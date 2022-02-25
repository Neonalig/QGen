#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using QGen.Lib.Common;

#endregion

namespace QGen.Lib;

/// <summary>
/// General extension methods and shorthand.
/// </summary>
public static class Extensions {

    /// <inheritdoc cref="TupleToString{T1,T2,T3}"/>
    public static string TupleToString<T1, T2>( this (T1, T2) Tuple ) => $"({Tuple.Item1}, {Tuple.Item2})";

    /// <summary>
    /// Converts the tuple to a <see cref="string"/> representation.
    /// </summary>
    /// <typeparam name="T1">The first element type.</typeparam>
    /// <typeparam name="T2">The second element type.</typeparam>
    /// <typeparam name="T3">The third element type.</typeparam>
    /// <param name="Tuple">The tuple.</param>
    /// <returns> A <see cref="string" /> that represents this tuple. </returns>
    public static string TupleToString<T1, T2, T3>( this (T1, T2, T3) Tuple ) => $"({Tuple.Item1}, {Tuple.Item2}, {Tuple.Item3})";

    /// <summary>
    /// Clamps the specified value within the given range.
    /// </summary>
    /// <typeparam name="T">The type of value to clamp. (i.e. <see cref="int"/>)</typeparam>
    /// <param name="Value">The value to clamp.</param>
    /// <param name="Min">The minimum possible value to return (inclusive).</param>
    /// <param name="Max">The maximum possible value to return (inclusive).</param>
    /// <returns>The <paramref name="Value"/> clamped within the range (<paramref name="Min"/>,<paramref name="Max"/>)</returns>
    public static T Clamp<T>( this T Value, T Min, T Max ) where T : IComparable<T> => Value.CompareTo(Min) < 0 ? Min : Value.CompareTo(Max) > 0 ? Max : Value;

    /// <summary>
    /// Returns all text before the given index.
    /// </summary>
    /// <param name="Str">The text to substring.</param>
    /// <param name="Index">The index to grab all text before.</param>
    /// <param name="Include">If <see langword="true" />, the character at the given <paramref name="Index"/> is also returned; otherwise all text <b>before</b> (not including) the <paramref name="Index"/> is returned.</param>
    /// <returns>The subtended string.</returns>
    public static string AllBefore( this string Str, int Index, bool Include = false ) => Str[..(Include ? Index + 1 : Index).Clamp(0, Str.Length)];

    /// <summary>
    /// Returns all text after the given index.
    /// </summary>
    /// <param name="Str">The text to substring.</param>
    /// <param name="Index">The index to grab all text after.</param>
    /// <param name="Include">If <see langword="true" />, the character at the given <paramref name="Index"/> is also returned; otherwise all text <b>after</b> (not including) the <paramref name="Index"/> is returned.</param>
    /// <returns>The subtended string.</returns>
    public static string AllAfter( this string Str, int Index, bool Include = false ) => Str[(Include ? Index : Index + 1).Clamp(0, Str.Length)..];

    /// <summary>
    /// Gets the resolved index (<paramref name="Cn"/> - <paramref name="In"/> if <see cref="Index.IsFromEnd"/>; otherwise just <paramref name="In"/>).
    /// </summary>
    /// <param name="In">The index.</param>
    /// <param name="Cn">The count.</param>
    /// <returns>The resolved index relative to the start/end of the range.</returns>
    public static int Resolve( this Index In, int Cn ) => In.IsFromEnd ? Cn - In.Value : In.Value;

    /// <summary>
    /// Gets the resolved start index (<paramref name="Cn"/> - <see cref="Range.Start"/> if <see cref="Index.IsFromEnd"/>; otherwise just <see cref="Range.Start"/>).
    /// </summary>
    /// <param name="Rn">The range.</param>
    /// <param name="Cn">The count.</param>
    /// <returns>The resolved starting index relative to the range.</returns>
    public static int ResolveStart( this Range Rn, int Cn ) => Rn.Start.Resolve(Cn);

    /// <summary>
    /// Gets the resolved start index (<paramref name="Cn"/> - <see cref="Range.End"/> if <see cref="Index.IsFromEnd"/>; otherwise just <see cref="Range.End"/>).
    /// </summary>
    /// <param name="Rn">The range.</param>
    /// <param name="Cn">The count.</param>
    /// <returns>The resolved end index relative to the range.</returns>
    public static int ResolveEnd( this Range Rn, int Cn ) => Rn.End.Resolve(Cn);

    /// <summary>
    /// Returns all elements in the given range.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Rn">The range of elements to return.</param>
    /// <param name="C">The number of items in the collection.</param>
    /// <returns>The elements in the given range.</returns>
    public static IEnumerable<T> WithinRange<T>( this IEnumerable<T> Enum, Range Rn, int C ) {
        int I = 0,
            St = Rn.ResolveStart(C),
            En = Rn.ResolveEnd(C);
        foreach ( T Item in Enum ) {
            if ( I > En ) {
                yield break;
            }
            if ( I < St ) {
                continue;
            }
            yield return Item;
            I++;
        }
    }

    /// <inheritdoc cref="WithinRange{T}(IEnumerable{T}, Range, int)"/>
    public static IEnumerable<T> WithinRange<T>( this IEnumerable<T> Enum, Range Rn ) => Enum.TryGetNonEnumeratedCount(out int C)
        ? WithinRange(Enum, Rn, C)
        : WithinRange(Enum.ToArray(), Rn);

    /// <inheritdoc cref="WithinRange{T}(IEnumerable{T}, Range, int)"/>
    public static IEnumerable<T> WithinRange<T>( this ICollection<T> Coll, Range Rn ) => WithinRange(Coll, Rn, Coll.Count);

    /// <inheritdoc cref="WithinRange{T}(IEnumerable{T}, Range, int)"/>
    public static IEnumerable<T> WithinRange<T>( this IList<T> Ls, Range Rn ) {
        int
            C = Ls.Count,
            St = Rn.ResolveStart(C),
            En = Rn.ResolveEnd(C);
        for ( int I = St; I <= En; I++ ) {
            yield return Ls[I];
        }
    }

    /// <inheritdoc cref="AllBefore{T}(IEnumerable{T}, int, int, bool)"/>
    public static IEnumerable<T> AllBefore<T>( this IList<T> Coll, int Index, bool Include = false ) => Coll.WithinRange(Range.StartAt((Include ? Index : Index + 1).Clamp(0, Coll.Count)));

    /// <inheritdoc cref="AllBefore{T}(IEnumerable{T}, int, int, bool)"/>
    public static IEnumerable<T> AllBefore<T>( this ICollection<T> Coll, int Index, bool Include = false ) => Coll.WithinRange(Range.StartAt((Include ? Index : Index + 1).Clamp(0, Coll.Count)));

    /// <summary>
    /// Returns all elements before the given index.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Coll">The collection to iterate.</param>
    /// <param name="Index">The index to grab all elements before.</param>
    /// <param name="Cn">The count of items in the collection.</param>
    /// <param name="Include">If <see langword="true" />, the element at the given <paramref name="Index"/> is also returned; otherwise all elements <b>before</b> (not including) the <paramref name="Index"/> is returned.</param>
    /// <returns>All elements before (/Including if <paramref name="Include"/> = <see langword="true"/>) the given <paramref name="Index"/>.</returns>
    public static IEnumerable<T> AllBefore<T>( this IEnumerable<T> Coll, int Index, int Cn, bool Include = false ) => Coll.WithinRange(Range.StartAt((Include ? Index : Index + 1).Clamp(0, Cn)), Cn);

    /// <inheritdoc cref="AllBefore{T}(IEnumerable{T}, int, int, bool)"/>
    public static IEnumerable<T> AllBefore<T>( this IEnumerable<T> Coll, int Index, bool Include = false ) => Coll.TryGetNonEnumeratedCount(out int C) ? AllBefore(Coll, Index, C, Include) : AllBefore(Coll.ToArray(), Index, Include);

    /// <inheritdoc cref="AllAfter{T}(IEnumerable{T}, int, int, bool)"/>
    public static IEnumerable<T> AllAfter<T>( this IList<T> Coll, int Index, bool Include = false ) => Coll.WithinRange((Include ? Index : Index + 1).Clamp(0, Coll.Count)..);

    /// <inheritdoc cref="AllAfter{T}(IEnumerable{T}, int, int, bool)"/>
    public static IEnumerable<T> AllAfter<T>( this ICollection<T> Coll, int Index, bool Include = false ) => Coll.WithinRange((Include ? Index : Index + 1).Clamp(0, Coll.Count)..);

    /// <summary>
    /// Returns all elements after the given index.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Coll">The collection to iterate.</param>
    /// <param name="Index">The index to grab all elements after.</param>
    /// <param name="Cn">The count of items in the collection.</param>
    /// <param name="Include">If <see langword="true" />, the element at the given <paramref name="Index"/> is also returned; otherwise all elements <b>after</b> (not including) the <paramref name="Index"/> is returned.</param>
    /// <returns>All elements after (/Including if <paramref name="Include"/> = <see langword="true"/>) the given <paramref name="Index"/>.</returns>
    public static IEnumerable<T> AllAfter<T>( this IEnumerable<T> Coll, int Index, int Cn, bool Include = false ) => Coll.WithinRange((Include ? Index : Index + 1).Clamp(0, Cn).., Cn);

    /// <inheritdoc cref="AllAfter{T}(IEnumerable{T}, int, int, bool)"/>
    public static IEnumerable<T> AllAfter<T>( this IEnumerable<T> Coll, int Index, bool Include = false ) => Coll.TryGetNonEnumeratedCount(out int C) ? AllAfter(Coll, Index, C, Include) : AllAfter(Coll.ToArray(), Index, Include);

    /// <summary>
    /// Constructs a dictionary from the given key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="Pairs">The pairs.</param>
    /// <returns>A new <see cref="Dictionary{TKey, TValue}"/> instance.</returns>
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>( this IEnumerable<KeyValuePair<TKey, TValue>> Pairs ) where TKey : notnull => new Dictionary<TKey, TValue>(Pairs);

    /// <summary>
    /// Determines if any items are in the collection, returning <see langword="true"/> if so.
    /// </summary>
    /// <typeparam name="TX">The type of the collection.</typeparam>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="C">The number of items in the collection.</param>
    /// <param name="Values">The values in the collection.</param>
    /// <returns><see langword="true"/> if there are any items in the collection; otherwise <see langword="false"/>.</returns>
    static bool Any<TX, T>( this TX Enum, int C, [NotNullWhen(true)] out TX? Values ) where TX : class, IEnumerable<T> {
        if ( C > 0 ) {
            Values = Enum;
            return true;
        }
        Values = null;
        return false;
    }

    /// <summary>
    /// Determines if any items are in the collection, returning <see langword="true"/> if so.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Coll">The collection to iterate.</param>
    /// <param name="Values">The values in the collection.</param>
    /// <returns><see langword="true"/> if there are any items in the collection; otherwise <see langword="false"/>.</returns>
    public static bool Any<T>( this ICollection<T> Coll, [NotNullWhen(true)] out ICollection<T>? Values ) => Any<ICollection<T>, T>(Coll, Coll.Count, out Values);

    /// <summary>
    /// Determines if any items are in the collection, returning <see langword="true"/> if so.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Values">The values in the collection.</param>
    /// <returns><see langword="true"/> if there are any items in the collection; otherwise <see langword="false"/>.</returns>
    public static bool Any<T>( this IEnumerable<T> Enum, [NotNullWhen(true)] out IEnumerable<T>? Values ) {
        if ( Enum.TryGetNonEnumeratedCount(out int C) ) {
            return Any<IEnumerable<T>, T>(Enum, C, out Values);
        }

        bool Res = Any(Enum.ToArray(), out ICollection<T>? FoundValues);
        Values = FoundValues;
        return Res;
    }

    /// <summary>
    /// Attempts to iterate the collection, returning whether any items are contained within.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Coll">The collection to iterate.</param>
    /// <param name="Any"><see langword="true"/> if there are any items in the collection; otherwise <see langword="false"/>.</param>
    /// <returns>The values in the collection.</returns>
    public static ICollection<T> TryIterate<T>( this ICollection<T> Coll, out bool Any ) {
        Any = Coll.Count > 0;
        return Coll;
    }

    /// <summary>
    /// Attempts to iterate the collection, returning whether any items are contained within.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Any"><see langword="true"/> if there are any items in the collection; otherwise <see langword="false"/>.</param>
    /// <returns>The values in the collection.</returns>
    public static IEnumerable<T> TryIterate<T>( this IEnumerable<T> Enum, out bool Any ) {
        if ( Enum.TryGetNonEnumeratedCount(out int C) ) {
            Any = C > 0;
            return Enum;
        }

        return TryIterate(Enum.ToArray(), out Any);
    }

    /// <summary>
    /// Iterates the specified enumerator.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <returns>The elements in the collection.</returns>
    public static IEnumerable<T> Iterate<T>( this IEnumerator<T> Enum ) {
        Enum.Reset();
        while ( Enum.MoveNext() ) {
            yield return Enum.Current;
        }
    }

    /// <summary>
    /// Attempts to iterate the collection, returning whether any items are contained within.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Any"><see langword="true"/> if there are any items in the collection; otherwise <see langword="false"/>.</param>
    /// <returns>The values in the collection.</returns>
    public static IEnumerable<T> TryIterate<T>( this IEnumerator<T> Enum, out bool Any ) {
        bool HadAny = false;

        IEnumerable<T> Gen() {
            foreach ( T Item in Enum.Iterate() ) {
                HadAny = true;
                yield return Item;
            }
        }

        IEnumerable<T> Return = Gen();
        Any = HadAny;
        return Return;
    }

    /// <inheritdoc cref="Index(int, bool)"/>
    /// <param name="Value">The index value. Must be zero or positive.</param>
    /// <param name="FromEnd">Indicates whether the index is from the start (<see langword="false"/>) or from the end (<see langword="true"/>) .</param>
    /// <returns>A new <see cref="Index"/> instance.</returns>
    public static Index GetIndex( this int Value, bool FromEnd = false ) => new Index(Value, FromEnd);

    /// <summary>
    /// Gets the range indicated by the <see cref="Capture"/>.
    /// </summary>
    /// <param name="Cpt">The capture.</param>
    /// <returns>The range of the captured text.</returns>
    public static Range GetRange( this Capture Cpt ) {
        int In = Cpt.Index;
        return new Range(In.GetIndex(), (In + Cpt.Length).GetIndex());
    }

    /// <summary>
    /// Gets the captured text.
    /// </summary>
    /// <param name="Cpt">The capture.</param>
    /// <param name="Text">The whole text that was matched against.</param>
    /// <returns>Only the captured text.</returns>
    public static string GetCapturedText( this Capture Cpt, string Text ) => Text[Cpt.GetRange()];

    /// <inheritdoc cref="string.Join(string, string?[])"/>
    /// <param name="Text">The collection of strings to join.</param>
    /// <param name="Separator">The text to place inbetween subsequent strings in the collection.</param>
    [return: NotNullIfNotNull("Text")]
    public static string? Join( this IEnumerable<string>? Text, string Separator ) => Text is null ? null : string.Join(Separator, Text);

    /// <inheritdoc cref="string.Join(string, string?[])"/>
    /// <param name="Text">The collection of objects to join. (<see cref="object.ToString()"/> will be invoked on each)</param>
    /// <param name="Separator">The text to place inbetween subsequent strings in the collection.</param>
    [return: NotNullIfNotNull("Text")]
    public static string? Join( this IEnumerable<object>? Text, string Separator ) => Text is null ? null : string.Join(Separator, Text);

    /// <inheritdoc cref="string.Join(string, string?[])"/>
    /// <typeparam name="T">The type of the objects.</typeparam>
    /// <param name="Text">The collection of objects to join. (the <paramref name="ToString"/> function will be invoked on each)</param>
    /// <param name="Separator">The text to place inbetween subsequent strings in the collection.</param>
    /// <param name="ToString">The function used to convert the object into a relevant <see cref="string"/> representation.</param>
    [return: NotNullIfNotNull("Text")]
    public static string? Join<T>( this IEnumerable<T>? Text, string Separator, Func<T, string> ToString ) => Text is null ? null : string.Join(Separator, Text.Select(ToString));

    /// <summary>
    /// Joins the strings into a single whole string.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <returns>The concatenation of all the specified strings.</returns>
    [return: NotNullIfNotNull("Text")]
    public static string? Join( this IEnumerable<string>? Text ) => Text is null ? null : string.Join(string.Empty, Text);

    /// <summary>
    /// Splits the specified text.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="Rn">The range.</param>
    /// <param name="Left">The text to the left of the range.</param>
    /// <param name="Mid">The text within the range.</param>
    /// <param name="Right">The text to the right of the range.</param>
    public static void Split( this string Text, Range Rn, out string? Left, out string? Mid, out string? Right ) {
        int L = Text.Length;
        int Start = Rn.ResolveStart(L);
        int End = Rn.ResolveEnd(L);
        Left = Start == 0 ? null : Text[..Start];
        Mid = Text[Start..End];
        Right = End >= L ? null : Text[(End + 1)..];
    }

    /// <summary>
    /// Splits the specified text.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="Index">The index.</param>
    /// <param name="Length">The length.</param>
    /// <param name="Left">The text to the left of the range.</param>
    /// <param name="Mid">The text within the range.</param>
    /// <param name="Right">The text to the right of the range.</param>
    public static void Split( this string Text, int Index, int Length, out string? Left, out string? Mid, out string? Right ) {
        Left = WithinRange(Text, 0, Index);
        Mid = WithinRange(Text, Index, Length);
        Right = WithinRange(Text, Index + Length, Text.Length - Index - Length);
    }

    /// <summary>
    /// Returns the text within the specified range.
    /// </summary>
    /// <param name="Text">The text to subtend.</param>
    /// <param name="Index">The starting index.</param>
    /// <param name="Length">The length of the text to return.</param>
    /// <returns>The text within the specified range, or <see langword="null"/> if the range is invalid.</returns>
    public static string? WithinRange( this string Text, int Index, int Length ) {
        int Ln = Text.Length, En = Index + Length;
        if ( Index < 0 || Index >= Ln || Length == 0 || En > Ln ) {
            return null;
        }
        return Text[Index..En];
    }

    /// <summary>
    /// Constructs a new <see cref="FileInfo"/> instance pointing to a file with the given name relative to the supplied parent directory.
    /// </summary>
    /// <param name="Dir">The parent directory.</param>
    /// <param name="Name">The child file name.</param>
    /// <returns>A new <see cref="FileInfo"/> instance equivalent to "<paramref name="Dir"/>/<paramref name="Name"/>".</returns>
    public static FileInfo GetSubFile( this DirectoryInfo Dir, string Name ) => new FileInfo(Path.Combine(Dir.FullName, Name));

    /// <summary>
    /// Constructs a new <see cref="FileInfo"/> instance pointing to the current path with a new extension.
    /// </summary>
    /// <param name="File">The current file path.</param>
    /// <param name="NewExt">The new extension. (i.e. '.mp3')</param>
    /// <returns>A new <see cref="FileInfo"/> instance.</returns>
    public static FileInfo WithExtension( this FileInfo File, string NewExt ) => new FileInfo($"{File.DirectoryName}{Path.GetFileNameWithoutExtension(File.Name)}.{NewExt.TrimStart('.')}");

    /// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// <param name="Source">The collection of items to iterate.</param>
    /// <param name="Selector">The transformation function.</param>
    public static TOut[] Select<TIn, TOut>( this TIn[] Source, Func<TIn, TOut> Selector ) {
        int L = Source.Length;
        TOut[] Out = new TOut[L];
        for ( int I = 0; I < L; I++ ) {
            Out[I] = Selector(Source[I]);
        }
        return Out;
    }

    /// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// <param name="Source">The collection of items to iterate.</param>
    /// <param name="Selector">The transformation function.</param>
    public static List<TOut> Select<TIn, TOut>( this IList<TIn> Source, Func<TIn, TOut> Selector ) {
        int L = Source.Count;
        List<TOut> Out = new List<TOut>(L);
        for ( int I = 0; I < L; I++ ) {
            Out[I] = Selector(Source[I]);
        }
        return Out;
    }

    /// <summary>
    /// Trims the string to the desired length.
    /// </summary>
    /// <param name="String">The string to trim.</param>
    /// <param name="Length">The desired string length. If the current string is already shorter, then no truncation is made.</param>
    /// <returns>The truncated string.</returns>
    public static string TrimToLength( this string String, int Length ) {
        int Ln = String.Length;
        return Length >= Ln ? String : String[..Length];
    }

    /// <summary>
    /// Trims the specified number of characters from the end of the string.
    /// </summary>
    /// <param name="String">The string to trim.</param>
    /// <param name="Chars">The amount of characters to remove. If greater than the length of the string, <see cref="string.Empty"/> is returned instead.</param>
    /// <returns>The truncated string.</returns>
    public static string TrimEnd( this string String, int Chars ) {
        int Ln = String.Length;
        return Chars >= Ln ? string.Empty : String[..(Ln - Chars)];
    }

    /// <summary>
    /// Trims the specified number of characters from the start of the string.
    /// </summary>
    /// <param name="String">The string to trim.</param>
    /// <param name="Chars">The amount of characters to remove. If greater than the length of the string, <see cref="string.Empty"/> is returned instead.</param>
    /// <returns>The truncated string.</returns>
    public static string TrimStart( this string String, int Chars ) => Chars <= 0 ? String : Chars > String.Length ? string.Empty : String[Chars..];

    /// <summary>
    /// Iterates all children nodes from the given point in the tree.
    /// </summary>
    /// <param name="Node">The current root node.</param>
    /// <param name="Recurse">If <see langword="true"/> children of children (etc.) are also returned; otherwise only the top level of children are returned.</param>
    /// <returns>All children nodes from the given point in the tree.</returns>
    public static IEnumerable<SyntaxNode> IterateAllNodes( this SyntaxNode Node, bool Recurse = true ) {
        foreach ( SyntaxNode Nd in Node.ChildNodes() ) {
            yield return Nd;
            if ( Recurse ) {
                foreach ( SyntaxNode N in IterateAllNodes(Nd, true) ) {
                    yield return N;
                }
            }
        }
    }

    /// <summary>
    /// Attempts to get the first token with the specified kind.
    /// </summary>
    /// <param name="Node">The node to search for tokens within.</param>
    /// <param name="Kind">The kind of token to search for.</param>
    /// <param name="Token">The found token, or <see langword="null"/> if <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the token was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetToken(this SyntaxNode Node, SyntaxKind Kind, [NotNullWhen(true)] out SyntaxToken? Token ) {
        foreach (SyntaxToken Tk in Node.ChildTokens() ) {
            if ( Tk.IsKind(Kind) ) {
                Token = Tk;
                return true;
            }
        }
        Token = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the first token.
    /// </summary>
    /// <param name="Node">The node to search for tokens within.</param>
    /// <param name="Token">The found token, or <see langword="null"/> if <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if any token was found; otherwise <see langword="false"/>.</returns>
    public static bool TryGetAnyToken( this SyntaxNode Node, [NotNullWhen(true)] out SyntaxToken? Token ) {
        foreach ( SyntaxToken Tk in Node.ChildTokens() ) {
            Token = Tk;
            return true;
        }
        Token = null;
        return false;
    }


    /// <summary>
    /// Asynchronously gets the <see cref="SyntaxTree"/> generated from the given C# file.
    /// </summary>
    /// <param name="ReadFile">The file to read.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    public static async Task<SyntaxTree> GetSyntaxTreeAsync( this FileInfo ReadFile, CancellationToken Token = default ) {
        await using ( FileStream FS = ReadFile.Open(FileMode.Open, FileAccess.Read) ) {
            SourceText ST = SourceText.From(FS);
            SyntaxTree Tree = CSharpSyntaxTree.ParseText(ST, path: ReadFile.FullName, cancellationToken: Token);
            return Tree;
        }
    }

    /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
    /// <param name="Dict">The dictionary to search through.</param>
    /// <param name="Key">The key to search for.</param>
    /// <param name="Value">The found value, or <see langword="null"/>.</param>
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
    public static bool TryGet<TKey, TValue>( this IDictionary<TKey, TValue> Dict, TKey Key, [NotNullWhen(true)] out TValue? Value) where TKey : notnull => Dict.TryGetValue(Key, out Value);
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.

    /// <summary>
    /// Attempts to get the first item in the collection that matches the given predicate.
    /// </summary>
    /// <typeparam name="TIn">The type of the input</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Selector">The selector.</param>
    /// <param name="Found">The found item, or <see langword="null"/> if <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if any of the items in the collection matched; otherwise <see langword="false"/>.</returns>
    public static bool TryGetFirst<TIn, TOut>(this IEnumerable<TIn> Enum, Func<TIn, Out<TOut>, bool> Selector, [NotNullWhen(true)] out TOut? Found ){
        Out<TOut?> Out = new Out<TOut?>(default);
        foreach ( TIn Item in Enum ) {
            if ( Selector(Item, Out!) ) {
                Found = Out.Value!;
                return true;
            }
            Out.Value = default;
        }
        Found = Out.Value;
        return false;
    }

    /// <summary>
    /// Attempts to get the first item in the collection that matches the given predicate.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Selector">The selector.</param>
    /// <param name="Found">The found item, or <see langword="null"/> if <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if any of the items in the collection matched; otherwise <see langword="false"/>.</returns>
    public static bool TryGetFirst<T>( [ItemNotNull] this IEnumerable<T> Enum, Func<T, bool> Selector, [NotNullWhen(true)] out T? Found ) {
        foreach ( T Item in Enum ) {
            if ( Selector(Item) ) {
                Found = Item!;
                return true;
            }
        }
        Found = default;
        return false;
    }

    /// <summary>
    /// Determines whether the left operand equals <b>any</b> of the specified <paramref name="Items"/>, returning <see langword="true"/> if so.
    /// </summary>
    /// <typeparam name="T">The type of item to check.</typeparam>
    /// <param name="Base">The left operand.</param>
    /// <param name="Items">The other items to check.</param>
    /// <returns><see langword="true"/> if <paramref name="Base"/> equals any of the specified <paramref name="Items"/>; otherwise <see langword="false"/>.</returns>
    public static bool Equals<T>(this T Base, params T[] Items) where T : IEquatable<T> {
        foreach ( T Item in Items ) {
            if ( Base.Equals(Item) ) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the file header.
    /// </summary>
    /// <param name="Generator">The generator.</param>
    /// <returns>The collection of strings representing the file header.</returns>
    public static string[] GetFileHeader( this IFileGenerator Generator ) => new [] {
        "//------------------------------------------------------------------------------",
        "// <auto-generated>",
        $"//     This code was generated by {Generator.Name.Or("an automatic source generator")}.",
        $"//     Runtime Version: {Generator.Version}",
        "//",
        "//     Changes to this file may cause incorrect behaviour and will be lost if",
        "//     the code is regenerated.",
        "// </auto-generated>",
        "//------------------------------------------------------------------------------",
        ""
    };

    /// <inheritdoc cref="IterateAsync{T}(IAsyncEnumerator{T}, CancellationToken)"/>
    public static async Task<List<T>> IterateAsync<T>( this IAsyncEnumerable<T> Enum, CancellationToken Token = new CancellationToken() ) => await IterateAsync(Enum.GetAsyncEnumerator(Token), Token);

    /// <summary>
    /// Asynchronously iterates through each member in the collection, storing the retrieved values in a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The asynchronous enumerable to iterate.</param>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>An asynchronous task which stores all items in the collection in a <see cref="List{T}"/>.</returns>
    public static async Task<List<T>> IterateAsync<T>( this IAsyncEnumerator<T> Enum, CancellationToken Token = new CancellationToken() ) {
        List<T> Ls = new List<T>();
        while ( true ) {
            if ( !await Enum.MoveNextAsync() ) { break; }
            Ls.Add(Enum.Current);
        }
        return Ls;
    }

    /// <summary>
    /// Converts the collection to a readonly instance.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to convert. If already of type <see cref="ReadOnlyCollection{T}"/>, nothing is changed. If value is <see langword="null"/>, <see langword="null"/> is returned. Otherwise, a new <see cref="ReadOnlyCollection{T}"/> is constructed.</param>
    /// <returns>A new/existing <see cref="ReadOnlyCollection{T}"/> based on the given collection.</returns>
    [return: NotNullIfNotNull("Enum")]
    public static ReadOnlyCollection<T>? ToReadOnly<T>( this IEnumerable<T>? Enum ) => Enum switch {
        ReadOnlyCollection<T> Coll => Coll,
        null                       => null,
        IList<T> Ls                => new ReadOnlyCollection<T>(Ls),
        _                          => new ReadOnlyCollection<T>(Enum.ToArray())
    };

    /// <summary>
    /// Applies the given cast to each item in the collection.
    /// </summary>
    /// <typeparam name="TIn">The type of the original input values.</typeparam>
    /// <typeparam name="TOut">The type to cast the values into.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="Cto">The casting function to invoke.</param>
    /// <returns>The cast items.</returns>
    public static IEnumerable<TOut> Cast<TIn, TOut>( this IEnumerable<TIn> Enum, Func<TIn, TOut> Cto ) {
        foreach ( TIn Item in Enum ) {
            yield return Cto(Item);
        }
    }

    /// <summary>
    /// Joins the two collections.
    /// </summary>
    /// <typeparam name="TA">The type of items in the first collection.</typeparam>
    /// <typeparam name="TB">The type of items in the second collection.</typeparam>
    /// <typeparam name="TJoint">The shared type of items overall.</typeparam>
    /// <param name="EnumA">The first collection to iterate and return.</param>
    /// <param name="EnumB">The second collection to iterate and return.</param>
    /// <returns>The concatenation of the two collections to a shared type.</returns>
    public static IEnumerable<TJoint> Join<TA, TB, TJoint>( this IEnumerable<TA> EnumA, IEnumerable<TB> EnumB ) where TJoint : TA, TB {
        foreach ( TA ItemA in EnumA ) {
            yield return (TJoint)ItemA!;
        }

        foreach ( TB ItemB in EnumB ) {
            yield return (TJoint)ItemB!;
        }
    }

    /// <summary>
    /// Joins the two collections.
    /// </summary>
    /// <param name="EnumA">The first collection to iterate and return.</param>
    /// <param name="EnumB">The second collection to iterate and return.</param>
    /// <returns>The concatenation of the two collections.</returns>
    public static IEnumerable Join(this IEnumerable EnumA, IEnumerable EnumB ) {
        foreach ( object? ItemA in EnumA ) {
            yield return ItemA;
        }
        foreach ( object? ItemB in EnumB ) {
            yield return ItemB;
        }
    }

    /// <summary>
    /// Joins the specified collections.
    /// </summary>
    /// <param name="EnumA">The first collection to iterate and return.</param>
    /// <param name="Others">The other collections to iterate and return in order.</param>
    /// <returns>The concatenation of the specified collections.</returns>
    public static IEnumerable Join( this IEnumerable EnumA, params IEnumerable[] Others ) {
        foreach ( object? ItemA in EnumA ) {
            yield return ItemA;
        }
        foreach ( IEnumerable Other in Others ) {
            foreach ( object? OtherItem in Other ) {
                yield return OtherItem;
            }
        }
    }

    /// <summary>
    /// Flattens the jagged collections into a single layer.
    /// </summary>
    /// <param name="Enums">The collections to iterate and return in order.</param>
    /// <returns>The elements of each collection in the specified grouping order.</returns>
    public static IEnumerable Flatten( this IEnumerable<IEnumerable> Enums ) {
        foreach (IEnumerable Enum in Enums ) {
            foreach (object? Item in Enum ) {
                yield return Item;
            }
        }
    }

    /// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
    /// <param name="Source">The collection to iterate.</param>
    /// <param name="Selector">The selector function.</param>
    public static IEnumerable<TResult> Select<TResult>( this IEnumerable Source, Func<object?, TResult> Selector ) {
        foreach ( object? Item in Source ) {
            yield return Selector(Item);
        }
    }

    /// <inheritdoc cref="CreateString{T}(IEnumerable{T}, Func{T, string})"/>
    public static string CreateString( this IEnumerable Enum ) => $"'{string.Join("', ", Enum)}'";

    /// <inheritdoc cref="CreateString{T}(IEnumerable{T}, Func{T, string})"/>
    public static string CreateString<T>( this IEnumerable<T> Enum ) => $"'{string.Join("', ", Enum)}'";

    /// <inheritdoc cref="CreateString{T}(IEnumerable{T}, Func{T, string})"/>
    public static string CreateString( this IEnumerable Enum, Func<object?, string> ToString ) => $"'{string.Join("', ", Enum.Select(ToString))}'";

    /// <summary>
    /// Creates a string from the given items in the collection.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="ToString">The function invoked for generating a string representation of each item.</param>
    /// <returns>A string representation of the entire collection. (i.e. <c>'5', '7.3', '-4.3234'</c>)</returns>
    public static string CreateString<T>( this IEnumerable<T> Enum, Func<T, string> ToString ) => $"'{string.Join("', ", Enum.Select(ToString))}'";

    /// <summary>
    /// Determines whether the two pointers are pointing to the same reference.
    /// </summary>
    /// <param name="A">The left operand.</param>
    /// <param name="B">The right operand.</param>
    /// <returns><see langword="true"/> if <paramref name="A"/>.<see cref="FileSystemInfo.FullName">FullName</see> equals <paramref name="B"/>.<see cref="FileSystemInfo.FullName">FullName</see> (case-insensitive); otherwise <see langword="false"/>.</returns>
    public static bool Equals( this FileSystemInfo A, FileSystemInfo B ) => A.FullName.Equals(B.FullName, StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// Grabs the specified amount of items from the collection.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The collection to iterate.</param>
    /// <param name="L">The maximum amount of items to return. If &lt;= 0, the function returns immediately.</param>
    /// <param name="Strict">If <see langword="true" />, an <see cref="InvalidOperationException"/> is thrown when the collection is less than the desired length; otherwise if less items are available than requested, the function will just return early.</param>
    /// <returns>The desired amount of items from the given collection, or less if the collection is smaller and <paramref name="Strict"/> is <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">$"Attempted to grab <paramref name="L"/> items from a collection with less than the required number of items.</exception>
    public static IEnumerable<T> Grab<T>( this IEnumerable<T> Enum, int L, bool Strict = false ) {
        if ( L <= 0 ) { yield break; }
        int I = 0;
        foreach ( T Item in Enum ) {
            yield return Item;
            I++;
            if ( I >= L ) { break; }
        }
        if ( Strict ) {
            throw new InvalidOperationException($"Attempted to grab {L} items from a collection with only {I} items.");
        }
    }

    /// <summary>
    /// Attempts to get the resultant value of the method, returning <see langword="true"/> if the result was a success.
    /// </summary>
    /// <param name="Result">The result.</param>
    /// <param name="Value">The resultant value.</param>
    /// <returns><see langword="true"/> if the result was a success; otherwise <see langword="false"/>.</returns>
    public static bool TryGetValue<T>( this Result<T> Result, [NotNullWhen(true)] out T? Value ) {
        if ( Result.Success ) {
            Value = Result.Value;
#pragma warning disable CS8762
            return true;
#pragma warning restore CS8762
        }
        Value = default;
        return false;
    }

    /// <inheritdoc cref="Result{T}.From(T)"/>
    public static Result<T> GetResult<T>( this T Value ) => Result<T>.From(Value);

    /// <inheritdoc cref="Result{T}.From(T, bool)"/>
    public static Result<T> GetResult<T>( this T Value, bool Success ) => Result<T>.From(Value, Success);

    /// <inheritdoc cref="Result{T}.From(T, bool, string)"/>
    public static Result<T> GetResult<T>( this T Value, bool Success, string Message ) => Result<T>.From(Value, Success, Message);

    /// <inheritdoc cref="Result{T}.From(T, bool, string)"/>
    public static Result<T> GetResult<T>( this bool Success, T? Value ) => Value is null ? Result<T>.UnexpectedError : Result<T>.From(Value, Success);

    /// <inheritdoc cref="Result{T}.From(T, bool, string)"/>
    public static Result<T> GetResult<T>( this bool Success, T? Value, string Message ) => Value is null ? Result<T>.UnexpectedError : Result<T>.From(Value, Success, Message);

    /// <summary>
    /// Attempts to enumerate the resultant values.
    /// </summary>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Result">The result.</param>
    /// <param name="Success">Whether the result was successful.</param>
    /// <returns>The collection to enumerate.</returns>
    public static IEnumerable<T> TryEnumerate<T>( this Result<IEnumerable<T>> Result, out bool Success ) {
        if ( !Result.Success ) {
            Success = false;
            return Enumerable.Empty<T>();
        }
        Success = true;
        return Result.Value;
    }

#pragma warning disable CS1998
    public static async IAsyncEnumerable<T> EmptyAsync<T>() {
        yield break;
    }
#pragma warning restore CS1998

#pragma warning disable CS1998
    public static async IAsyncEnumerable<T> AsAsync<T>(this IEnumerable<T> Enum ) {
        foreach ( T Item in Enum ) {
            yield return Item;
        }
    }
#pragma warning restore CS1998

    static async Task<IReadOnlyList<T>> GetAwaiterTask<T>( this IAsyncEnumerator<T> Enum, CancellationToken Token = new CancellationToken() ) {
        if ( Token.IsCancellationRequested ) { return Array.Empty<T>().ToReadOnly(); }
        List<T> Ls = new List<T>();
        while ( true ) {
            if ( Token.IsCancellationRequested ) { break; }
            if ( !await Enum.MoveNextAsync() ) {
                break;
            }
            if ( Token.IsCancellationRequested ) { break; }
            Ls.Add(Enum.Current);
        }
        return Ls.AsReadOnly();
    }

    static async Task<IReadOnlyList<T>> GetAwaiterTask<T>( this IAsyncEnumerable<T> Enum, CancellationToken Token = new CancellationToken() ) => await GetAwaiterTask(Enum.GetAsyncEnumerator(Token), Token);

    public static TaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>( this IAsyncEnumerator<T> Enum ) => Enum.GetAwaiterTask().GetAwaiter();
    public static TaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>( this IAsyncEnumerator<T> Enum, CancellationToken Token ) => Enum.GetAwaiterTask(Token).GetAwaiter();

    public static TaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>( this IAsyncEnumerable<T> Enum ) => Enum.GetAwaiterTask().GetAwaiter();
    public static TaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>( this IAsyncEnumerable<T> Enum, CancellationToken Token ) => Enum.GetAwaiterTask(Token).GetAwaiter();

    public static TaskAwaiter<T> GetAwaiter<T>( this TaskAwaiter<T> Awaiter ) => Awaiter;
    public static TaskAwaiter GetAwaiter( this TaskAwaiter Awaiter ) => Awaiter;

    public static TaskAwaiter<T> GetAwaiter<T>( this LazyAsync<T> Async ) => Async.GetValueAsync().GetAwaiter();
    public static TaskAwaiter<T> GetAwaiter<T>( this LazyAsync<T> Async, CancellationToken Token ) => Async.GetValueAsync(Token).GetAwaiter();

    public static TaskAwaiter<ReadOnlyCollection<T>> GetAwaiter<T>( this LazyAsyncEnumerable<T> Async, CancellationToken Token = new CancellationToken() ) => Async.GetValuesAsync(Token).GetAwaiter();

    /// <summary>
    /// Logs the current result to the debug trace listeners.
    /// </summary>
    /// <typeparam name="T">The resultant value type.</typeparam>
    /// <param name="Result">The result to log.</param>
    [Conditional("DEBUG")]
    public static void Log<T>( this IResult<T> Result ) => Debug.WriteLine($"The method executed with the message: \"{Result.Message}\". (Value: '{Result.Value?.ToString() ?? "<null>"}')", Result.Success ? "SUCCESS" : "ERROR");

    /// <summary>
    /// Logs the current result to the debug trace listeners.
    /// </summary>
    /// <typeparam name="T">The resultant value type.</typeparam>
    /// <param name="Result">The result to log.</param>
    /// <param name="WhenSuccess">If <see langword="false"/>, successful results are <b>not</b> logged; otherwise all results are logged regardless of being successful or not.</param>
    [Conditional("DEBUG")]
    public static void Log<T>( this IResult<T> Result, bool WhenSuccess ) {
        if ( !Result.Success || WhenSuccess ) {
            Log(Result);
        }
    }

    /// <summary>
    /// Logs the current result to the debug trace listeners.
    /// </summary>
    /// <param name="Result">The result to log.</param>
    [Conditional("DEBUG")]
    public static void Log( this IResult Result ) {
#if DEBUG
        Debug.WriteLine(Result.DbgVal switch {
            { } V => $"The method executed with the message: \"{Result.Message}\". (Value: '{V}')",
            _     => $"The method executed with the message: \"{Result.Message}\"."
        }, Result.Success ? "SUCCESS" : "ERROR");
#else
        Debug.WriteLine($"The method executed with the message: \"{Result.Message}\".", Result.Success ? "SUCCESS" : "ERROR");
#endif
    }

    /// <summary>
    /// Logs the current result to the debug trace listeners.
    /// </summary>
    /// <param name="Result">The result to log.</param>
    /// <param name="WhenSuccess">If <see langword="false"/>, successful results are <b>not</b> logged; otherwise all results are logged regardless of being successful or not.</param>
    [Conditional("DEBUG")]
    public static void Log( this IResult Result, bool WhenSuccess ) {
        if ( !Result.Success || WhenSuccess ) {
            Log(Result);
        }
    }

    /// <summary>
    /// Gets the invariant culture equivalent of the specified character.
    /// </summary>
    /// <param name="C">The character, or <see langword="null"/>.</param>
    /// <returns>The culture-invariant character, or <see langword="null"/> if the specified character is also <see langword="null"/>.</returns>
    /// <seealso cref="char.ToUpperInvariant(char)"/>
    /// <seealso cref="char.ToLowerInvariant(char)"/>
    [return: NotNullIfNotNull("C")]
    public static char? ToInvariant( this char? C ) => C.HasValue ? GetInv(C.Value) : null;

    static char GetInv( this char C ) => char.IsUpper(C) ? char.ToUpperInvariant(C) : char.ToLowerInvariant(C);

    /// <summary>
    /// Gets the invariant culture equivalent of the specified string.
    /// </summary>
    /// <param name="Str">The string, or <see langword="null"/>.</param>
    /// <returns>The culture-invariant string, or <see langword="null"/> if the specified string is also <see langword="null"/>.</returns>
    /// <seealso cref="char.ToUpperInvariant(char)"/>
    /// <seealso cref="char.ToLowerInvariant(char)"/>
    [return: NotNullIfNotNull("Str")]
    public static string? ToInvariant( this string? Str ) => Str is null ? null : new string(Str.ToCharArray().Select(GetInv));

    /// <summary>
    /// Gets the equivalent asynchronous enumerable.
    /// </summary>
    /// <typeparam name="T">The value containing type.</typeparam>
    /// <param name="Task">The asynchronous task which returns the collection.</param>
    /// <returns>The equivalent asynchronous enumerable collection.</returns>
    public static async IAsyncEnumerable<T> GetAsyncEnumerable<T>( this Task<IEnumerable<T>> Task ) {
        IEnumerable<T> Enum = await Task;
        foreach ( T Item in Enum ) {
            yield return Item;
        }
    }

    /// <summary>
    /// Chains the specified actions.
    /// </summary>
    /// <param name="Act">The initial action to invoke.</param>
    /// <param name="Ex">The secondary action to invoke.</param>
    /// <returns>A new action which invokes the first action followed by the second.</returns>
    public static Action Chain( this Action Act, Action Ex ) {
        void Method() {
            Act();
            Ex();
        }
        return Method;
    }

    /// <summary>
    /// Chains the specified actions.
    /// </summary>
    /// <param name="Actions">The collection of actions to invoke.</param>
    /// <returns>A new action which invokes each action in the collection in order.</returns>
    public static Action Chain( this IEnumerable<Action> Actions ) {
        void Method() {
            foreach ( Action Act in Actions ) {
                Act();
            }
        }
        return Method;
    }

    static IEnumerable<T> With<T>( this T A, params T[] Others ) {
        yield return A;
        foreach ( T Other in Others ) {
            yield return Other;
        }
    }

    /// <inheritdoc cref="Chain(IEnumerable{Action})"/>
    /// <param name="Act">The initial action to invoke.</param>
    /// <param name="Others">The additional collection of actions to invoke.</param>
    public static Action Chain( this Action Act, params Action[] Others ) => Chain(Act.With(Others));

    /// <summary>
    /// Chains the specified actions.
    /// </summary>
    /// <typeparam name="T">The action parameter type.</typeparam>
    /// <param name="Actions">The collection of actions to invoke.</param>
    /// <returns>A new action which invokes each action in the collection in order.</returns>
    public static Action<T> Chain<T>( this IEnumerable<Action<T>> Actions ) {
        void Method( T Val ) {
            foreach ( Action<T> Act in Actions ) {
                Act(Val);
            }
        }
        return Method;
    }

    /// <inheritdoc cref="Chain{T}(IEnumerable{Action{T}})"/>
    /// <param name="Act">The initial action to invoke.</param>
    /// <param name="Others">The additional collection of actions to invoke.</param>
    public static Action<T> Chain<T>( this Action<T> Act, params Action<T>[] Others ) => Chain(Act.With(Others));

    /// <summary>
    /// Chains the specified actions.
    /// </summary>
    /// <typeparam name="T">The action parameter type.</typeparam>
    /// <param name="Act">The initial action.</param>
    /// <param name="Ex">The secondary action.</param>
    /// <returns>A new action which invokes the first action followed by the second.</returns>
    public static Action<T> Chain<T>( this Action<T> Act, Action<T> Ex ) {
        void Method( T Val ) {
            Act(Val);
            Ex(Val);
        }
        return Method;
    }

    /// <summary>
    /// Chains a transformation to the end of the function.
    /// </summary>
    /// <typeparam name="TIn">The initial function value type.</typeparam>
    /// <typeparam name="TOut">The final function value type.</typeparam>
    /// <param name="Func">The function.</param>
    /// <param name="Transformation">The transformation to apply after the function is invoked.</param>
    /// <returns>The original function with a transformation that is applied after invocation.</returns>
    public static Func<TOut> Transform<TIn, TOut>( this Func<TIn> Func, Func<TIn, TOut> Transformation ) {
        TOut Trans() => Transformation(Func());
        return Trans;
    }

    /// <summary>
    /// Forces the type parameter of the result.
    /// </summary>
    /// <typeparam name="T">The resultant value type.</typeparam>
    /// <param name="Res">The result.</param>
    /// <returns>A new <see cref="Result{T}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Types on results can only be forced when the result indicates an unsuccessful method execution. Successful executions should just return the result directly.</exception>
    public static Result<T> ForceType<T>( this Result Res ) {
        if ( Res.Success ) {
            throw new InvalidOperationException("Types on results can only be forced when the result indicates an unsuccessful method execution. Successful executions should just return the result directly.");
        }
        return new Result<T>(false, Res.Message, default!);
    }

    /// <summary>
    /// Forces the type parameter of the result.
    /// </summary>
    /// <typeparam name="TIn">The original resultant value type.</typeparam>
    /// <typeparam name="TOut">The new resultant value type.</typeparam>
    /// <param name="Res">The result.</param>
    /// <returns>A new <see cref="Result{T}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Types on results can only be forced when the result indicates an unsuccessful method execution. Successful executions should just return the result directly.</exception>
    public static Result<TOut> ForceType<TIn, TOut>( this Result<TIn> Res ) {
        if ( Res.Success ) {
            throw new InvalidOperationException("Types on results can only be forced when the result indicates an unsuccessful method execution. Successful executions should just return the result directly.");
        }
        return new Result<TOut>(false, Res.Message, default!);
    }

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with the same message, but with a different value type.
    /// </summary>
    /// <typeparam name="T">The resultant value type.</typeparam>
    /// <param name="Res">The original result.</param>
    /// <param name="Value">The new value.</param>
    /// <returns>A new <see cref="Result{T}"/> instance.</returns>
    public static Result<T> With<T>( this Result Res, T Value ) => new Result<T>(Res.Success, Res.Message, Value);

    /// <summary>
    /// Returns a new <see cref="Result{T}"/> with the same message, but with a different value type.
    /// </summary>
    /// <typeparam name="TIn">The original resultant value type.</typeparam>
    /// <typeparam name="TOut">The new resultant value type.</typeparam>
    /// <param name="Res">The original result.</param>
    /// <param name="Value">The new value.</param>
    /// <returns>A new <see cref="Result{T}"/> instance.</returns>
    public static Result<TOut> With<TIn, TOut>( this Result<TIn> Res, TOut Value ) => new Result<TOut>(Res.Success, Res.Message, Value);

    /// <summary>
    /// Chains the next result to the queue.
    /// </summary>
    /// <typeparam name="TIn">The first resultant value type.</typeparam>
    /// <typeparam name="TOut">The second resultant value type.</typeparam>
    /// <param name="Res">The initial result. If successful, <paramref name="Func"/> is invoked with the value, and that new result is returned; otherwise the first error message is returned.</param>
    /// <param name="Func">The next result generating function.</param>
    /// <returns>The first result if unsuccessful; otherwise the second result via the first result's successful value.</returns>
    public static Result<TOut> Then<TIn, TOut>( this Result<TIn> Res, Func<TIn, Result<TOut>> Func ) => Res.TryGetValue(out TIn? Val)
        ? Func(Val)
        : Res.ForceType<TIn, TOut>();

    /// <inheritdoc cref="Then{TIn, TOut}(Result{TIn}, Func{TIn, Result{TOut}})"/>
    public static Result<TOut> Then<TIn, TOut>( this Result<TIn> Res, Func<TIn, TOut?> Func ) {
        if ( Res.TryGetValue(out TIn? Val) ) {
            TOut? NewResVal = Func(Val);
            return (NewResVal is not null).GetResult(NewResVal);
        }
        return new Result<TOut>(false, Res.Message, default!);
    }

    /// <inheritdoc cref="string.IsNullOrEmpty(string?)"/>
    /// <param name="Value">The text to check.</param>
    public static bool IsNullOrEmpty( [NotNullWhen(false)] this string? Value ) => string.IsNullOrEmpty(Value);

    /// <inheritdoc cref="string.IsNullOrWhiteSpace(string?)"/>
    /// <param name="Value">The text to check.</param>
    public static bool IsNullOrWhiteSpace( [NotNullWhen(false)] this string? Value ) => string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Returns the first non-<see langword="null"/> value.
    /// </summary>
    /// <typeparam name="T">The value return type.</typeparam>
    /// <param name="Val">The first value.</param>
    /// <param name="Other">The other (fallback) value.</param>
    /// <returns>The first non-<see langword="null"/> value.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static T Or<T>( this T? Val, [DisallowNull] T Other ) => Val is null ? Other : Val;

    /// <summary>
    /// Returns the first non-<see langword="null"/> string.
    /// </summary>
    /// <param name="Val">The first string.</param>
    /// <param name="Other">The other (fallback) string.</param>
    /// <returns>The first non-<see langword="null"/> string.</returns>
    public static string Or(this string? Val, string Other) => Val.IsNullOrEmpty() ? Other : Val;

    /// <summary>
    /// Returns the first non-<see langword="null"/> value.
    /// </summary>
    /// <typeparam name="T">The value return type.</typeparam>
    /// <param name="Vals">The values to check.</param>
    /// <param name="Fallback">The fallback value if all else is <see langword="null"/>.</param>
    /// <returns>The first non-<see langword="null"/> value in the collection.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static T Or<T>( this IEnumerable<T?> Vals, [DisallowNull] T Fallback ) => Vals.FirstOrDefault(Vl => Vl is not null) is { } V ? V : Fallback;

    /// <summary>
    /// Returns the first non-<see langword="null"/> string.
    /// </summary>
    /// <param name="Vals">The strings to check.</param>
    /// <param name="Fallback">The fallback string if all else is <see langword="null"/>.</param>
    /// <returns>The first non-<see langword="null"/> string in the collection.</returns>
    public static string Or( this IEnumerable<string?> Vals, string Fallback ) => Vals.FirstOrDefault(Vl => !Vl.IsNullOrEmpty()) is { } V ? V : Fallback;

    /// <summary>
    /// Returns the first non-<see langword="null"/> value.
    /// </summary>
    /// <typeparam name="T">The value return type.</typeparam>
    /// <param name="Val">The first value to check.</param>
    /// <param name="Fallback">The fallback value if all else is <see langword="null"/>.</param>
    /// <param name="OtherVals">The other values to check.</param>
    /// <returns>The first non-<see langword="null"/> value in the collection.</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    public static T Or<T>( this T? Val, [DisallowNull] T Fallback, params T?[] OtherVals ) => Val.With(OtherVals).Or(Fallback);

    /// <summary>
    /// Returns the first non-<see langword="null"/> string.
    /// </summary>
    /// <param name="Val">The first string to check.</param>
    /// <param name="Fallback">The fallback string if all else is <see langword="null"/>.</param>
    /// <param name="OtherVals">The other strings to check.</param>
    /// <returns>The first non-<see langword="null"/> string in the collection.</returns>
    public static string Or( this string? Val, string Fallback, params string?[] OtherVals ) => Val.With(OtherVals).Or(Fallback);

    /// <summary>
    /// Returns the input typed as <see cref="IEnumerable"/>.
    /// </summary>
    /// <param name="Enum">The enumerable collection to return.</param>
    /// <returns>The input sequence typed as <see cref="IEnumerable"/>.</returns>
    public static IEnumerable AsEnum( this IEnumerable Enum ) => Enum;

    /// <inheritdoc cref="Enumerable.AsEnumerable{TSource}(IEnumerable{TSource})"/>
    /// <typeparam name="T">The collection containing type.</typeparam>
    /// <param name="Enum">The enumerable collection to return.</param>
    public static IEnumerable<T> AsEnum<T>( this IEnumerable<T> Enum ) => Enum;

    /// <inheritdoc cref="Task.FromResult{TResult}(TResult)"/>
    /// <param name="Result">The result of the task.</param>
    public static Task<T> AsTask<T>( this T Result ) => Task.FromResult(Result);

    /// <summary>
    /// Method responsible for resolving a root folder from a generator provider.
    /// </summary>
    /// <param name="RequestedRootFolder">The requested root folder.</param>
    /// <returns>The result of the method execution.</returns>
    public delegate Result<DirectoryInfo> ResolvePath( string RequestedRootFolder );

    /// <summary>
    /// Resolves the root folder from the given generator provider.
    /// </summary>
    /// <param name="Provider">The provider.</param>
    /// <param name="Alt">The alternative method to invoke if the default root folder path cannot be resolved.</param>
    /// <returns>The result of the method execution.</returns>
    public static Result<DirectoryInfo> ResolveRootFolder( this IGeneratorProvider Provider, ResolvePath Alt ) {
        if ( Provider.DefaultRootFolder is { } Path
             && Path.GetDirectory(true).TryGetValue(out DirectoryInfo? RootFolder) ) {
            return RootFolder;
        }
        return Alt(Provider.RequestedRootFolder);
    }

    /// <summary>
    /// Gets the <see cref="DirectoryInfo"/> instance pointing to the designated path.
    /// </summary>
    /// <param name="Path">The path.</param>
    /// <param name="MustExist">Whether the directory must actually exist to be returned. If valid, but not found, a <see cref="Result.DirectoryNotFound(string)"/> result will be returned instead.</param>
    /// <returns>The result of the method execution.</returns>
    public static Result<DirectoryInfo> GetDirectory( this string Path, bool MustExist = false ) => Result<DirectoryInfo>.TryCatch(() => ConstructDirectoryPointer(Path, MustExist));

    static DirectoryInfo ConstructDirectoryPointer( string Path, bool MustExist ) {
        DirectoryInfo File = new DirectoryInfo(Path);
        if ( MustExist && !File.Exists ) {
            return Result<DirectoryInfo>.DirectoryNotFound(Path);
        }
        return File;
    }

    /// <summary>
    /// Gets the <see cref="FileInfo"/> instance pointing to the designated path.
    /// </summary>
    /// <param name="Path">The path.</param>
    /// <param name="MustExist">Whether the file must actually exist to be returned. If valid, but not found, a <see cref="Result.FileNotFound(string)"/> result will be returned instead.</param>
    /// <returns>The result of the method execution.</returns>
    public static Result<FileInfo> GetFile( this string Path, bool MustExist = false ) => Result<FileInfo>.TryCatch(() => ConstructFilePointer(Path, MustExist));

    static FileInfo ConstructFilePointer( string Path, bool MustExist ) {
        FileInfo File = new FileInfo(Path);
        if ( MustExist && !File.Exists ) {
            return Result<FileInfo>.FileNotFound(Path);
        }
        return File;
    }

    /// <summary>
    /// Determines and returns the first successful result.
    /// </summary>
    /// <typeparam name="T">The resultant value type.</typeparam>
    /// <param name="A">The first result to check.</param>
    /// <param name="B">The second result to check.</param>
    /// <returns>The first successful result.</returns>
    public static Result<T> Or<T>( this Result<T> A, Result<T> B ) => A.Success ? A : B;

    /// <summary>
    /// Determines and returns the first successful result in the collection.
    /// </summary>
    /// <typeparam name="T">The resultant value type.</typeparam>
    /// <param name="Results">The collection of results to check.</param>
    /// <returns>The first successful result in the collection.</returns>
    public static Result<T> Or<T>( this IEnumerable<Result<T>> Results ) {
        Result<T>? Last = null;
        foreach ( Result<T> Result in Results ) {
            Last = Result;
            if ( Result.Success ) {
                return Result;
            }
        }
        if ( !Last.HasValue ) {
            throw new ArgumentException("The collection of results was empty.", nameof(Results));
        }
        return Last.Value;
    }

    /// <inheritdoc cref="Or{T}(IEnumerable{Result{T}})"/>
    /// <param name="A">The initial result to check.</param>
    /// <param name="Others">The other results to check.</param>
    public static Result<T> Or<T>( this Result<T> A, params Result<T>[] Others ) => Or(A.With(Others: Others));
}