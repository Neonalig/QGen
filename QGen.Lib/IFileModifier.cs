﻿#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using QGen.Lib.Common;

#endregion

namespace QGen.Lib;

/// <summary>
/// Represents a source generator which modifies an existing template, creating a new file.
/// </summary>
public interface IFileModifier {

    /// <summary>
    /// Gets the name of this modifier utility.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the version of this modifier utility.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    Version Version { get; }

    /// <summary>
    /// Gets the requested path.
    /// </summary>
    /// <remarks>Extensions (such as .cs and .auto.cs) should not be specified.</remarks>
    /// <value>
    /// The requested path.
    /// </value>
    string RequestedPath { get; }

    /// <summary>
    /// Reads the specified path, caching any relevant info.
    /// </summary>
    /// <param name="Path">The path.</param>
    /// <param name="Tree">The tree.</param>
    /// <param name="Root">The compilation root.</param>
    /// <param name="Generators">The collection of relevant generators.</param>
    /// <param name="Token">The cancellation token.</param>
    Task ReadAsync( FileInfo Path, SyntaxTree Tree, CompilationUnitSyntax Root, Out<IEnumerable<IMatchGenerator>> Generators, CancellationToken Token );

}
