#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Text.RegularExpressions;

using QGen.Lib.Common;

#endregion

namespace QGen.Lib;

/// <summary>
/// Represents a portion of a source generator for a specific match name.
/// </summary>
public interface IMatchGenerator {

    /// <summary>
    /// Gets the name of the match generator.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Generates the code given the specified match.
    /// </summary>
    /// <param name="Match">The matched text.</param>
    /// <param name="Line">The line of text that was matched.</param>
    /// <returns>The portion of text to replace the matched text with.</returns>
    Result<string> Generate( Match Match, string Line );

}