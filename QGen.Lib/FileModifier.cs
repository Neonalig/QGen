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
/// Represents a source generator which modifies an existing template, creating a new file.
/// </summary>
public abstract class FileModifier : ITemplateModifier {

    /// <summary>
    /// Gets the relative path to the desired template file.
    /// </summary>
    /// <value>
    /// The relative path to the template file.
    /// </value>
    public abstract string TemplatePath { get; }
    //Value will be used in ParsedDirectory.TryGetFile(string, out FileSystem.ParsedFile?)

    /// <summary><inheritdoc cref="ITemplateModifier.LookupAsync(ParsedDirectory, ParsedFile, CancellationToken)"/></summary>
    /// <param name="RootDirectory"><inheritdoc cref="ITemplateModifier.LookupAsync(ParsedDirectory, ParsedFile, CancellationToken)"/></param>
    /// <param name="TemplateFile">The template file to read data from.</param>
    /// <param name="DestinationFile"><inheritdoc cref="ITemplateModifier.LookupAsync(ParsedDirectory, ParsedFile, CancellationToken)"/></param>
    /// <param name="Token"><inheritdoc cref="ITemplateModifier.LookupAsync(ParsedDirectory, ParsedFile, CancellationToken)"/></param>
    /// <returns>The lines of text to write in the destination file.</returns>
    public abstract Task<Result<IEnumerable<IMatchGenerator>>> LookupAsync( ParsedDirectory RootDirectory, ParsedFile TemplateFile, ParsedFile DestinationFile, CancellationToken Token );

    /// <summary>
    /// Gets the template file.
    /// </summary>
    /// <param name="RootDirectory">The root directory.</param>
    /// <param name="TemplatePath">The relative path to the template file.</param>
    /// <returns>The result of the method execution.</returns>
    public static Result<ParsedFile> GetTemplateFile( ParsedDirectory RootDirectory, string TemplatePath ) => RootDirectory.TryGetFile(TemplatePath, out ParsedFile? PF).GetResult(PF);

    #region IFileGenerator Implementation

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract Version Version { get; }

    #endregion

    #region ITemplateModifier Implementation

    /// <inheritdoc />
    public abstract string DestinationPath { get; }

    /// <inheritdoc />
    public virtual async Task<Result<(IEnumerable<string> Lines, IEnumerable<IMatchGenerator> Generators)>> LookupAsync( ParsedDirectory RootDirectory, ParsedFile DestinationFile, CancellationToken Token ) {
        if (!RootDirectory.TryGetFile(TemplatePath, out ParsedFile? TemplateFile) ) {
            return Result<(IEnumerable<string>, IEnumerable<IMatchGenerator>)>.FileNotFound(TemplatePath);
        }

        IEnumerable<string> Lines = await TemplateFile.Lines;
        Result<IEnumerable<IMatchGenerator>> GeneratorsResult = await LookupAsync(RootDirectory, TemplateFile, DestinationFile, Token);

        return GeneratorsResult.Then(Generators => (Lines, Generators));
    }

    #endregion
}