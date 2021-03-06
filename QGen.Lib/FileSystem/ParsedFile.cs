#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using QGen.Lib.Common;

#endregion

namespace QGen.Lib.FileSystem;

/// <summary>
/// Represents a C# source file which gets dynamically parsed in portions upon request.
/// </summary>
public class ParsedFile : FileSystemInfo {

    /// <inheritdoc cref="ParsedFile(FileInfo, ParsedDirectory)"/>
    /// <remarks>If this <see cref="ParsedFile"/> is constructed relative to a <see cref="ParsedDirectory"/>, use the other constructor (<see langword="new"/> <see cref="ParsedFile(FileInfo, ParsedDirectory)"/>) to avoid excessive, unnecessary memory allocations.</remarks>
    public ParsedFile( FileInfo Path ) : this (Path, new ParsedDirectory(Path.Directory!)) { }

    #region FileSystemInfo Implementation

    /// <inheritdoc />
    public override bool Exists => Path.Exists;

    /// <inheritdoc />
    public override void Delete() => Path.Delete();

    /// <inheritdoc />
    public override string Name => Path.Name;

    /// <inheritdoc />
    public override string FullName => Path.FullName;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="ParsedFile"/> class.
    /// </summary>
    /// <param name="Path">The path to the C# source file.</param>
    /// <param name="Parent">The parent directory.</param>
    public ParsedFile( FileInfo Path, ParsedDirectory Parent ) {
        this.Path = Path;
        this.Parent = Parent;
        Lines = new LazyAsyncEnumerable<string>(async () => await RequestLinesAsync());
        Text = new LazyAsync<string>(RequestTextAsync);
        SourceText = new LazyAsync<SourceText>(RequestSourceTextAsync);
        SyntaxTree = new LazyAsync<SyntaxTree>(RequestSyntaxTreeAsync);
        CompilationUnitRoot = new LazyAsync<CompilationUnitSyntax>(RequestCompilationUnitRootAsync);
        RootNode = new LazyAsync<SyntaxNode>(RequestRootNodeAsync);
        ChildNodesAndTokens = new LazyAsyncEnumerable<SyntaxNodeOrToken>(RequestChildNodesAndTokensAsync);
        ChildNodes = new LazyAsyncEnumerable<SyntaxNode>(RequestChildNodesAsync);

        //LazyAsync and LazyAsyncEnumerable types ensure only the needed values are constructed.
        //  i.e. if the user requests for 'SyntaxTree', Lines > Text > SourceText > SyntaxTree is constructed.
        //  i.e. if the user requests for 'ChildNodesAndTokens', Lines > Text > SourceText > SyntaxTree > RootNode > ChildNodesAndTokens is constructed.

        //On subsequent uses (i.e. 'SyntaxTree' was requested, then 'RootNode' was requested), prior constructed values are cached.
        //  i.e. 'SyntaxTree' constructs Lines > Text > SourceText > SyntaxTree
        //  i.e. 'RootNode' only constructs RootNode, since SyntaxTree (and prior) was already constructed.
    }

    /// <summary>
    /// The path to the C# source file.
    /// </summary>
    public readonly FileInfo Path;

    /// <summary>
    /// The parent folder.
    /// </summary>
    public readonly ParsedDirectory Parent;

    /// <summary>
    /// The lines of text within the file.
    /// </summary>
    /// <seealso cref="Text"/>
    public LazyAsyncEnumerable<string> Lines;
    async Task<string[]> RequestLinesAsync( CancellationToken Token = default ) => await File.ReadAllLinesAsync(Path.FullName, Token);

    /// <summary>
    /// The text contained within the file.
    /// </summary>
    /// <seealso cref="Lines"/>
    public LazyAsync<string> Text;
    async Task<string> RequestTextAsync( CancellationToken Token = default ) => (await Lines.GetValuesAsync(Token)).Join("\r\n");

    /// <summary>
    /// The parsed source text from the contents within the file.
    /// </summary>
    public LazyAsync<SourceText> SourceText;
    async Task<SourceText> RequestSourceTextAsync( CancellationToken Token = default ) => Microsoft.CodeAnalysis.Text.SourceText.From(await Text.GetValueAsync(Token));

    /// <summary>
    /// The parsed syntax tree from the contents within the file.
    /// </summary>
    public LazyAsync<SyntaxTree> SyntaxTree;
    async Task<SyntaxTree> RequestSyntaxTreeAsync( CancellationToken Token = default ) => CSharpSyntaxTree.ParseText(await SourceText.GetValueAsync(Token), path: Path.FullName, cancellationToken: Token);

    /// <summary>
    /// The compilation unit syntax root from the parsed syntax tree.
    /// </summary>
    /// <seealso cref="SyntaxTree"/>
    public LazyAsync<CompilationUnitSyntax> CompilationUnitRoot;
    async Task<CompilationUnitSyntax> RequestCompilationUnitRootAsync( CancellationToken Token = default ) => (await SyntaxTree.GetValueAsync(Token)).GetCompilationUnitRoot(Token);

    /// <summary>
    /// The root node from the parsed syntax tree.
    /// </summary>
    /// <seealso cref="SyntaxTree"/>
    public LazyAsync<SyntaxNode> RootNode;
    async Task<SyntaxNode> RequestRootNodeAsync( CancellationToken Token = default ) => await (await SyntaxTree.GetValueAsync(Token)).GetRootAsync(Token);

    /// <summary>
    /// The list of child nodes and tokens of the <see cref="RootNode"/> (where each element is a <see cref="SyntaxNodeOrToken"/> element).
    /// </summary>
    /// <seealso cref="ChildNodes"/>
    public LazyAsyncEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens;
    async IAsyncEnumerable<SyntaxNodeOrToken> RequestChildNodesAndTokensAsync( [EnumeratorCancellation] CancellationToken Token = default ) {
        foreach ( SyntaxNodeOrToken Child in (await RequestRootNodeAsync(Token)).ChildNodesAndTokens() ) {
            yield return Child;
        }
    }

    /// <summary>
    /// The list of child nodes of the <see cref="RootNode"/> (where each element is a <see cref="SyntaxNode"/> element).
    /// </summary>
    /// <seealso cref="ChildNodesAndTokens"/>
    public LazyAsyncEnumerable<SyntaxNode> ChildNodes;
    async IAsyncEnumerable<SyntaxNode> RequestChildNodesAsync( [EnumeratorCancellation] CancellationToken Token = default ) {
        foreach ( SyntaxNode Child in (await RequestRootNodeAsync(Token)).ChildNodes() ) {
            yield return Child;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Path.Name;
}