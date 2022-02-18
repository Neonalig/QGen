#region Copyright (C) 2017-2022  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Text.RegularExpressions;

#endregion

namespace QGen.Core;

/// <summary>
/// Represents a portion of a source generator for a specific match name.
/// </summary>
internal interface IMatchGenerator {

    /// <summary>
    /// Gets the name.
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
    void Generate( Match Match, string Line );

}