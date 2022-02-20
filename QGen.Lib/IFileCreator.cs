using System.Text.RegularExpressions;

using QGen.Lib.Common;
using QGen.Lib.FileSystem;

namespace QGen.Lib;

/// <summary>
/// Represents a source generator which creates a new file.
/// </summary>
public interface IFileCreator {

    /// <summary>
    /// Gets the name of this source generator.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the version of this source generator.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    Version Version { get; }

    /// <summary>
    /// Asynchronously runs the source generator inside of the specified root directory, looking for any desired files and returning the relevant <see cref="IMatchGenerator"/>s to execute.
    /// </summary>
    /// <param name="RootDirectory">The root directory.</param>
    /// <returns>The result of the method execution.</returns>
    Task<Result<IEnumerable<IMatchGenerator>>> LookupAsync( ParsedDirectory RootDirectory );

}

public interface IMatchGeneratorStartupContext {

    /// <summary>
    /// Gets the generator.
    /// </summary>
    /// <value>
    /// The generator.
    /// </value>
    IMatchGenerator Generator { get; }

    /// <inheritdoc cref="IMatchGenerator.Generate(Match, string)"/>
    Result<string> Generate(Match Match, string Line);

}

/// <summary>
/// A simple <see cref="IMatchGeneratorStartupContext"/> implementation with no passthrough data.
/// </summary>
/// <seealso cref="IMatchGeneratorStartupContext" />
public readonly struct MatchGeneratorSimpleStartupContext : IMatchGeneratorStartupContext {

    /// <summary>
    /// Initialises a new instance of the <see cref="MatchGeneratorSimpleStartupContext"/> struct.
    /// </summary>
    /// <param name="Generator">The generator.</param>
    public MatchGeneratorSimpleStartupContext( IMatchGenerator Generator ) => this.Generator = Generator;

    #region Implementation of IMatchGeneratorStartupContext

    /// <inheritdoc />
    public IMatchGenerator Generator { get; }

    /// <inheritdoc />
    public Result<string> Generate(Match Match, string Line) => Generator.Generate(Match, Line);

    #endregion
}