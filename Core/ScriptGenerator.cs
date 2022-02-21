#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using QGen.Lib.Common;
using QGen.Lib.FileSystem;

#endregion

namespace QGen.Core;

/// <summary>
/// Provides methods to generate assemblies from raw script files.
/// </summary>
public class ScriptGenerator {

    public static async Task<Result> GenerateAsync( DirectoryInfo RootDirectory, IEnumerable<IFileGenerator> Generators ) {
        //TODO: Below
        //  IFileModifier clones a template file, then modifies the template data stored within.
        //  IFileCreator reads multiple files in the root directory, then can create numerous new files with that information.
        ParsedDirectory Dir = new ParsedDirectory(RootDirectory);
        foreach ( IFileGenerator Generator in Generators ) {
            switch ( Generator ) {
                case IFileModifier Mod:
                    if ( !Dir.TryGetFile(Mod.TemplatePath, out ParsedFile? TemplateFile) ) {
                        return Result.FilePathInvalid(Mod.TemplatePath);
                    }
                    if ( !TemplateFile.Exists ) {
                        return Result.FileNotFound(Mod.TemplatePath);
                    }
                    if ( !Dir.TryGetFile(Mod.DestinationPath, out ParsedFile? DestinationFile) ) {
                        return Result.FilePathInvalid(Mod.TemplatePath);
                    }

                    if ( !(await Mod.LookupAsync(Dir, TemplateFile, DestinationFile)).TryGetValue(out IEnumerable<IMatchGenerator>? MatchGens ) ) {
                        return Result.LookupFailed(Mod);
                    }

                    if ( DestinationFile.Exists ) { DestinationFile.Delete(); }
                    await using ( StreamWriter SW = File.CreateText(DestinationFile.FullName) ) {
                        foreach ( string Line in SourceGenerator.Generate(await TemplateFile.Lines.GetValueAsync(), MatchGens) ) {
                            await SW.WriteLineAsync(Line);
                        }
                        await SW.FlushAsync();
                    }
                    break;
                case IFileCreator Crt:
                    int I = 0;
                    foreach ( string Dest in Crt.DestinationFiles ) {
                        if ( !Dir.TryGetFile(Dest, out ParsedFile? DestFile) ) {
                            return Result.FilePathInvalid(Dest);
                        }

                        if ( DestFile.Exists ) { DestFile.Delete(); }


                        I++;
                    }
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Caches the names of the given file modifiers.
    /// </summary>
    /// <param name="Modifiers">The modifiers.</param>
    /// <returns>A new <see cref="Dictionary{TKey, TValue}"/> instance.</returns>
    internal static Dictionary<string, IFileModifier> CacheNames( IEnumerable<IFileModifier> Modifiers ) => Modifiers.ToDictionary(FM => FM.RequestedPath.ToUpperInvariant());

    /// <inheritdoc cref="GenerateAsync(DirectoryInfo, SearchOption, IDictionary{string, IFileModifier}, CancellationToken)"/>
    public static async Task GenerateAsync( DirectoryInfo Directory, SearchOption SearchOption, IEnumerable<IFileModifier> Modifiers, CancellationToken Token = default ) => await GenerateAsync(Directory, SearchOption, CacheNames(Modifiers), Token);

    /// <summary>
    /// Asynchronously iterates through all *.acs template files, and automatically generates modifications where available.
    /// </summary>
    /// <param name="Directory">The directory to search through.</param>
    /// <param name="SearchOption">The search option to use for finding files. (i.e. <see cref="SearchOption.AllDirectories"/> or <see cref="SearchOption.TopDirectoryOnly"/>)</param>
    /// <param name="Modifiers">The file moddifiers available.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    public static async Task GenerateAsync( DirectoryInfo Directory, SearchOption SearchOption, IDictionary<string, IFileModifier> Modifiers, CancellationToken Token = default ) {
        foreach ( FileInfo TemplateFile in Directory.GetFiles("*.auto.cs", SearchOption) ) {
            string Nm = TemplateFile.Name.TrimEnd(8);
            if ( Modifiers.TryGet(Nm.ToUpperInvariant(), out IFileModifier? Modifier) ) {
                FileInfo DestFile = TemplateFile.Directory!.GetSubFile(Nm + ".cs");
                await GenerateAsync(TemplateFile, DestFile, Modifier, Token);
            }
        }
    }

    /// <summary>
    /// Asynchronously generates a new script file based on the given template and modifiers.
    /// </summary>
    /// <param name="TemplateFile">The file template to base the modifications off of.</param>
    /// <param name="DestFile">The destination file to create.</param>
    /// <param name="Modifier">The file modifier to use.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    public static async Task GenerateAsync( FileInfo TemplateFile, FileInfo DestFile, IFileModifier Modifier, CancellationToken Token = default ) {
        //Debug.WriteLine($"Reading template file ({TemplateFile.FullName})...");
        //string[] Lns = File.ReadAllLines(TemplateFile.FullName);
        string[] Lns = await File.ReadAllLinesAsync(TemplateFile.FullName, Token);
        string LnsJn = Lns.Join("\r\n");
        //Debug.WriteLine("Constructing text source...");
        SourceText ST = SourceText.From(LnsJn);
        //Debug.WriteLine("Constructing syntax tree...");
        SyntaxTree Tree = CSharpSyntaxTree.ParseText(ST, path: DestFile.FullName, cancellationToken: Token);
        //Debug.WriteLine("Getting syntax tree root...");
        CompilationUnitSyntax Root = Tree.GetCompilationUnitRoot(Token);
        if ( DestFile.Exists ) {
            //Debug.WriteLine("Deleting pre-existing destination file...");
            DestFile.Delete();
        }
        //Debug.WriteLine("Creating destination file...");
        await using ( StreamWriter SW = File.CreateText(DestFile.FullName) ) {
            //Debug.WriteLine("Modifying lines...");
            await foreach ( string Ln in ModifyAsync(TemplateFile, Lns, Tree, Root, Modifier, Token) ) {
                //Debug.WriteLine($"Writing modified line '{Ln}'...");
                await SW.WriteLineAsync(Ln.AsMemory(), Token);
            }
            //Debug.WriteLine("Flushing...");
            await SW.FlushAsync();
        }
    }

    /// <summary>
    /// Modifies the given script file.
    /// </summary>
    /// <param name="TemplateFile">The file template to base the modifications off of.</param>
    /// <param name="Lines">The lines of text contained within the template file.</param>
    /// <param name="Tree">The parsed syntax tree.</param>
    /// <param name="Root">The compilation root.</param>
    /// <param name="Modifier">The file modifier to use.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The modified lines of text.</returns>
    public static async IAsyncEnumerable<string> ModifyAsync( FileInfo TemplateFile, string[] Lines, SyntaxTree Tree, CompilationUnitSyntax Root, IFileModifier Modifier, [EnumeratorCancellation] CancellationToken Token = default ) {
        Debug.WriteLine("Retrieving file header...");
        foreach ( string Ln in Modifier.GetFileHeader() ) {
            yield return Ln;
        }

        Debug.WriteLine("Reading modifier..");
        Out<IEnumerable<IMatchGenerator>> Generators = new Out<IEnumerable<IMatchGenerator>>(Array.Empty<IMatchGenerator>());
        await Modifier.ReadAsync(TemplateFile, Tree, Root, Generators, Token);
        Debug.WriteLine($"Got generators '{Generators.Value.Join("', '")}'.");

        Debug.WriteLine("Generating source...");
        foreach ( string ModLn in SourceGenerator.Generate(Lines, Generators.Value) ) {
            Debug.WriteLine($"\t[SC]: {ModLn}");
            yield return ModLn;
        }
    }

}
