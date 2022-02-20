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
/// Represents a source generator which creates a new file.
/// </summary>
public interface IFileCreator : IFileGenerator {

    /// <summary>
    /// Gets the relative paths to the desired destination files to create.
    /// </summary>
    /// <value>
    /// The relative paths to the destination files.
    /// </value>
    IEnumerable<string> DestinationFiles { get; }

    /// <summary>
    /// Asynchronously runs the source generator inside of the specified root directory, looking for any desired files, storing the requested information, and generating a new file from that stored information.
    /// </summary>
    /// <param name="RootDirectory">The root directory.</param>
    /// <param name="CreationIndex">The current index of which destination file is being created.</param>
    /// <param name="DestinationFile">The path to the destination file location.</param>
    /// <returns>The lines of text to write in the destination file.</returns>
    Task<Result<IEnumerable<string>>> CreateAsync( ParsedDirectory RootDirectory, int CreationIndex, FileInfo DestinationFile );

}