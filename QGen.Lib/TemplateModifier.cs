#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using QGen.Lib.Common;
using QGen.Lib.FileSystem;

#endregion

namespace QGen.Lib;

/// <summary>
/// <see cref="IFileCreator"/> implementation which generates files based on an internal collection of strings.
/// </summary>
/// <seealso cref="IFileCreator" />
public abstract class TemplateModifier : ITemplateModifier {

    #region IFileGenerator Implementation

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract Version Version { get; }

    #endregion

    /// <summary>
    /// Gets the lines of text in the template.
    /// </summary>
    /// <value>
    /// The template lines.
    /// </value>
    public abstract IAsyncEnumerable<string> TemplateLines { get; }

    /// <inheritdoc cref="FileModifier.LookupAsync(ParsedDirectory, ParsedFile, ParsedFile, CancellationToken)"/>
    /// <param name="RootDirectory">The root directory.</param>
    /// <param name="DestinationFile">The path to the desired destination file.</param>
    /// <param name="Token">The cancellation token.</param>
    public abstract Task<Result<IEnumerable<IMatchGenerator>>> LookupAsync( ParsedDirectory RootDirectory, ParsedFile DestinationFile, CancellationToken Token );

    #region ITemplateModifier Implementation

    /// <inheritdoc />
    public abstract string DestinationPath { get; }

    /// <inheritdoc />
    async Task<Result<(IEnumerable<string> Lines, IEnumerable<IMatchGenerator> Generators)>> ITemplateModifier.LookupAsync( ParsedDirectory RootDirectory, ParsedFile DestinationFile, CancellationToken Token ) {
        IEnumerable<string> Lines = await TemplateLines.GetAwaiter(Token);
        Result<IEnumerable<IMatchGenerator>> GeneratorsResult = await LookupAsync(RootDirectory, DestinationFile, Token);

        return GeneratorsResult.Then(Generators => (Lines, Generators));
    }

    #endregion
}
