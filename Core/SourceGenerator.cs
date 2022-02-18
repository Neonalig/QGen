#region Copyright (C) 2017-2022  Cody Bock
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
internal class SourceGenerator {

    /// <summary>
    /// The pre-compiled <see cref="Regex"/> instance used to find $(...) regions for automatic generation.
    /// </summary>
    internal static readonly Regex MatchRegex = new Regex("\\$\\((?<Name>[a-zA-Z0-9]+)\\)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Caches the names of the given match generators.
    /// </summary>
    /// <param name="Generators">The generators.</param>
    /// <returns>A new <see cref="Dictionary{TKey, TValue}"/> instance.</returns>
    internal static Dictionary<string, IMatchGenerator> CacheNames( IEnumerable<IMatchGenerator> Generators ) => Generators.ToDictionary(MG => MG.Name);

    /// <inheritdoc cref="Generate(IEnumerable{string}, Dictionary{string, IMatchGenerator})"/>
    internal static IEnumerable<string> Generate( IEnumerable<string> Lines, IEnumerable<IMatchGenerator> Generators ) => Generate(Lines, CacheNames(Generators));

    /// <summary>
    /// Generates new content from the given the collection of <see cref="IMatchGenerator"/>s.
    /// </summary>
    /// <param name="Lines">The original lines of text.</param>
    /// <param name="Generators">The generators.</param>
    /// <returns>The new lines of text.</returns>
    internal static IEnumerable<string> Generate( IEnumerable<string> Lines, Dictionary<string, IMatchGenerator> Generators ) {
        foreach ( string Line in Lines ) {
            yield return Generate(Line, Generators);
        }
    }

    /// <inheritdoc cref="GenerateSegments(string, Dictionary{string, IMatchGenerator})"/>
    /// <returns>The new line.</returns>
    internal static string Generate( string Line, Dictionary<string, IMatchGenerator> Generators ) => GenerateSegments(Line, Generators).Join();

    /// <summary>
    /// Generates the new line given the collection of <see cref="IMatchGenerator"/>s.
    /// </summary>
    /// <param name="Line">The original line.</param>
    /// <param name="Generators">The generators.</param>
    /// <returns>The segments of the new line.</returns>
    internal static IEnumerable<string> GenerateSegments( string Line, Dictionary<string, IMatchGenerator> Generators ) {
        bool Success;
        foreach ( Match M in MatchRegex.Matches(Line).TryIterate(out Success) ) {
            if ( M.Success ) {
                string MT = M.Groups["Name"].Value; //Just the match value ;; i.e. 'ValName'
                //Debug.WriteLine($"From '{Line}', captured '{T}' (Name '{MT}').");

                Line.Split(M.Index, M.Length, out string? Prior, out string? Match, out string? Next);
                if ( Prior is not null ) { yield return Prior; }
                Debug.WriteLine($"\t'{Prior ?? "<null>"}';;'{Match ?? "<null>"}';;'{Next ?? "<null>"}'");

                if ( Generators.TryGetValue(MT, out IMatchGenerator? Generator) ) {
                    yield return Generator.Generate(M, Line);
                }

                if ( Next is null ) { yield break; }
                Line = Next;
                //continue;
            }
        }
        if ( Success ) {
            if ( !string.IsNullOrEmpty(Line) ) {
                yield return Line;
            }
        } else {
            //Debug.WriteLine($"No matches found in {Line}.");
            yield return Line;
        }
    }

    //TODO: Partial reflection (Dynamic w/ .cs file (circa CSharpCodeProvider?); Unity compiled assemblies?)

}