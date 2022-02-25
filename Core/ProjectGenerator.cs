#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.IO;

using Ookii.Dialogs.Wpf;

using QGen.Lib.Common;
using QGen.Lib.FileSystem;

#endregion

namespace QGen.Core;

/// <summary>
/// Handles source generation on a per-project basis.
/// </summary>
/// <remarks>
/// <list type="bullet">
///     <item>
///         <term> File Generation </term>
///         <description> <see cref="FileGenerator"/> </description>
///     </item>
///     <item>
///         <term> Project Generation </term>
///         <description> <see cref="GenerateAsync(IGeneratorProvider, CancellationToken)"/> </description>
///     </item>
/// </list>
/// </remarks>
/// <seealso cref="FileGenerator"/>
/// <seealso cref="IGeneratorProvider"/>
/// <seealso cref="IFileGenerator"/>
public class ProjectGenerator {

    /// <summary>
    /// Asynchronously generates the source files defined in the <see cref="IGeneratorProvider"/>.
    /// </summary>
    /// <param name="Provider">The generator provider.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The result of the method execution.</returns>
    public static async Task<Result> GenerateAsync( IGeneratorProvider Provider, CancellationToken Token = new() ) {
        Result<DirectoryInfo> Dir = Provider.ResolveRootFolder(ResolvePath);
        switch ( Dir.Success ) {
            case true:
                Result<IEnumerable<IFileGenerator>> Generators = await Provider.GetGeneratorsAsync(Token);
                switch ( Generators.Success ) {
                    case true:
                        Result GenRes = await GenerateAsync(Dir.Value, Generators.Value, Token);
                        switch ( GenRes.Success ) {
                            case true:
                                Debug.WriteLine("Source generators ran successfully!");
                                break;
                            default:
                                Debug.WriteLine($"Source generation failed with the result: '{Generators.Message}'.");
                                break;
                        }
                        break;
                    default:
                        Debug.WriteLine($"Source generator retrieval failed with the result: '{Generators.Message}'.");
                        break;
                }
                break;
            
            default:
                Debug.WriteLine($"Root folder resolution failed with the result: '{Dir.Message}'.");
                break;
        }

        return Result.Successful;
    }

    /// <summary>
    /// Resolves the path root folder path.
    /// </summary>
    /// <param name="RequestedRootFolder">The snippet of the path.</param>
    /// <returns>The result of the method execution.</returns>
    public static Result<DirectoryInfo> ResolvePath( string RequestedRootFolder ) {
        VistaFolderBrowserDialog VFBD = new VistaFolderBrowserDialog {
            Description = $"Select the '{RequestedRootFolder}' path.",
            Multiselect = false,
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true
        };

        return VFBD.ShowDialog() == true
            ? VFBD.SelectedPath.GetDirectory(true)
            : Result<DirectoryInfo>.UserCancelledDialog;
    }

    /// <summary>
    /// Asynchronously runs the source file generators defined in the provided collection.
    /// </summary>
    /// <param name="RootDirectory">The root directory.</param>
    /// <param name="Generators">The generators.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The result of the method execution.</returns>
    /// <exception cref="System.NotSupportedException">Source generation from the given generator is not supported. Ensure class type derives from either <see cref="IFileCreator"/> or <see cref="ITemplateModifier"/>.</exception>
    public static async Task<Result> GenerateAsync( DirectoryInfo RootDirectory, IEnumerable<IFileGenerator> Generators, CancellationToken Token = new CancellationToken() ) {
        ParsedDirectory Dir = new ParsedDirectory(RootDirectory);
        foreach ( IFileGenerator Generator in Generators ) {
            if ( Token.IsCancellationRequested ) { return Result.Cancelled(true); }
            switch ( Generator ) {
                case ITemplateModifier Mod: {
                    if ( !Dir.TryGetFilePointer(Mod.DestinationPath, out ParsedFile? DestinationFile) ) {
                        return Result.FilePathInvalid(Mod.DestinationPath);
                    }

                    if ( !(await Mod.LookupAsync(Dir, DestinationFile, Token)).TryGetValue(out (IEnumerable<string> Lines, IEnumerable<IMatchGenerator> MatchGens) Res ) ) {
                        return Result.LookupFailed(Mod);
                    }

                    return await FileGenerator.GenerateFileAsync(DestinationFile, Mod.GetFileHeader(), FileGenerator.Generate(Res.Lines, Res.MatchGens));
                }
                case IFileCreator Crt: {
                    int I = 0;
                    foreach ( string Destination in Crt.DestinationFiles ) {
                        if ( !Dir.TryGetFilePointer(Destination, out ParsedFile? DestinationFile) ) {
                            return Result.FilePathInvalid(Destination);
                        }

                        Result<IEnumerable<string>> CreationResult = await Crt.CreateAsync(Dir, I, DestinationFile, Token);
                        if ( !CreationResult.Success ) {
                            return CreationResult;
                        }

                        Result GenerationResult = await FileGenerator.GenerateFileAsync(DestinationFile, Crt.GetFileHeader(), CreationResult.Value);
                        if ( !GenerationResult.Success ) {
                            return GenerationResult;
                        }

                        I++;
                    }
                    break;
                }
                default:
                    throw new NotSupportedException($"Source generation via type '{Generator.GetType()}' is not supported. Ensure class type derives from either {nameof(IFileCreator)} or {nameof(ITemplateModifier)}.");
            }
        }
        return Result.Successful;
    }

}
