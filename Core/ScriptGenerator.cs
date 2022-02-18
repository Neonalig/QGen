using System.IO;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace QGen.Core;

/// <summary>
/// Provides methods to generate assemblies from raw script files.
/// </summary>
public class ScriptGenerator {

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
        await using ( FileStream FS = TemplateFile.Open(FileMode.Open, FileAccess.Read) ) {
            SourceText ST = SourceText.From(FS);
            SyntaxTree Tree = CSharpSyntaxTree.ParseText(ST, path: DestFile.FullName, cancellationToken: Token);
            CompilationUnitSyntax Root = Tree.GetCompilationUnitRoot(Token);
            if ( DestFile.Exists ) { DestFile.Delete(); }
            await using ( StreamWriter SW = File.CreateText(DestFile.FullName) ) {
                await foreach ( string Ln in ModifyAsync(TemplateFile, Tree, Root, Modifier, Token) ) {
                    await SW.WriteLineAsync(Ln.AsMemory(), Token);
                }
                await SW.FlushAsync();
            }
            //await FS.FlushAsync(Token);
        }
    }

    /// <summary>
    /// Modifies the given script file.
    /// </summary>
    /// <param name="TemplateFile">The file template to base the modifications off of.</param>
    /// <param name="Tree">The parsed syntax tree.</param>
    /// <param name="Root">The compilation root.</param>
    /// <param name="Modifier">The file modifier to use.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The modified lines of text.</returns>
    public static async IAsyncEnumerable<string> ModifyAsync( FileInfo TemplateFile, SyntaxTree Tree, CompilationUnitSyntax Root, IFileModifier Modifier, [EnumeratorCancellation] CancellationToken Token = default ) {
        foreach ( string Ln in Modifier.GetFileHeader() ) {
            yield return Ln;
        }

        Out<IEnumerable<IMatchGenerator>> Generators = new Out<IEnumerable<IMatchGenerator>>(Array.Empty<IMatchGenerator>());
        await Modifier.ReadAsync(TemplateFile, Tree, Root, Generators, Token);

        foreach ( string ModLn in SourceGenerator.Generate(await File.ReadAllLinesAsync(TemplateFile.FullName, Token), Generators.Value) ) {
            yield return ModLn;
        }
    }

}
