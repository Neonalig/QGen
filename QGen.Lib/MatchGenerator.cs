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
/// Simple <see cref="IMatchGenerator"/> implementation which simply invokes the provided function.
/// </summary>
/// <seealso cref="IMatchGenerator" />
public class MatchGenerator : IMatchGenerator {

    /// <summary>
    /// Initialises a new instance of the <see cref="MatchGenerator"/> struct.
    /// </summary>
    /// <param name="Name">The name.</param>
    /// <param name="Text">The replacement text.</param>
    public MatchGenerator( string Name, string Text ) {
        this.Name = Name;
        Func = (_, _) => Text;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="MatchGenerator"/> struct.
    /// </summary>
    /// <param name="Name">The name.</param>
    /// <param name="Func">The replacement function.</param>
    public MatchGenerator( string Name, Replace Func ) {
        this.Name = Name;
        this.Func = Func;
    }

    /// <inheritdoc cref="IMatchGenerator.Generate(Match, string)"/>
    public delegate string Replace( Match Match, string Line );

    /// <summary>
    /// Gets the match replacement function.
    /// </summary>
    /// <value>
    /// The replacement function.
    /// </value>
    public Replace Func { get; }

    #region IMatchGenerator Implementation

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Result<string> Generate( Match Match, string Line ) => Func(Match, Line);

    #endregion
}