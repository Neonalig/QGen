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
public interface IFileModifier : IFileGenerator {

    /// <summary>
    /// Gets the relative path to the desired template file.
    /// </summary>
    /// <value>
    /// The relative path to the template file.
    /// </value>
    string TemplatePath { get; }
    //Value will be used in ParsedDirectory.TryGetFile(string, out FileSystem.ParsedFile?)

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
    /// <param name="TemplateFile">The template file to read data from.</param>
    /// <param name="DestinationFile">The path to the desired destination file.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The collection of match generators to use on the destination file based upon the contents of the template file.</returns>
    Task<Result<IEnumerable<IMatchGenerator>>> LookupAsync( ParsedDirectory RootDirectory, ParsedFile TemplateFile, ParsedFile DestinationFile, CancellationToken Token );

}
