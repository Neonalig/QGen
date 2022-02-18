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
/// The source generator class.
/// </summary>
internal class Generator {

    /// <summary>
    /// The pre-compiled <see cref="Regex"/> instance used to find $(...) regions for automatic generation.
    /// </summary>
    internal static readonly Regex MatchRegex = new Regex("\\$\\((?<Name>[a-zA-Z0-9]+)\\)", RegexOptions.Compiled);

    //TODO: Text Before, Text After (Match.index)
    //TODO: Partial reflection (Dynamic w/ .cs file (circa CSharpCodeProvider?); Unity compiled assemblies?)
    //TODO: Either reflection, or reading from raw/non-compiled script file

}