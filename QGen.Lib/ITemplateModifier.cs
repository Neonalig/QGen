#region Copyright (C) 2017-2022  Cody Bock

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html

#endregion

using QGen.Lib.Common;
using QGen.Lib.FileSystem;

namespace QGen.Lib;

/// <summary>
/// Represents a source generator which modifies an existing template, creating a new file.
/// </summary>
public interface ITemplateModifier : IFileGenerator {

    /// <summary>
    /// Gets the relative path to the desired destination file.
    /// </summary>
    /// <value>
    /// The relative path to the destination file.
    /// </value>
    string DestinationPath { get; }

    /// <summary>
    /// Asynchronously runs the source generator inside of the specified root directory, looking for any desired files and returning the relevant <see cref="IMatchGenerator"/>s to execute.
    /// </summary>
    /// <param name="RootDirectory">The root directory.</param>
    /// <param name="DestinationFile">The path to the desired destination file.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The template's lines of text, and the collection of match generators to use on those lines to transform the template.</returns>
    Task<Result<(IEnumerable<string> Lines, IEnumerable<IMatchGenerator> Generators)>> LookupAsync( ParsedDirectory RootDirectory, ParsedFile DestinationFile, CancellationToken Token );

}