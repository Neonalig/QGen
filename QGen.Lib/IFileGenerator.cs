#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

namespace QGen.Lib;

/// <summary>
/// Represents a source generator.
/// </summary>
public interface IFileGenerator {

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

}