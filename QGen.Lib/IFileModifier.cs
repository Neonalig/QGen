using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QGen.Lib;

/// <summary>
/// Represents a source generator which modifies an existing template, creating a new file.
/// </summary>
public interface IFileModifier {

    /// <summary>
    /// Gets the name of this modifier utility.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the version of this modifier utility.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    Version Version { get; }

    /// <summary>
    /// Gets the requested path.
    /// </summary>
    /// <remarks>Extensions (such as .cs and .auto.cs) should not be specified.</remarks>
    /// <value>
    /// The requested path.
    /// </value>
    string RequestedPath { get; }

    /// <summary>
    /// Reads the specified path, caching any relevant info.
    /// </summary>
    /// <param name="Path">The path.</param>
    /// <param name="Tree">The tree.</param>
    /// <param name="Root">The compilation root.</param>
    /// <param name="Generators">The collection of relevant generators.</param>
    /// <param name="Token">The cancellation token.</param>
    Task ReadAsync( FileInfo Path, SyntaxTree Tree, CompilationUnitSyntax Root, Out<IEnumerable<IMatchGenerator>> Generators, CancellationToken Token );

}
