#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

#endregion

namespace QGen.Core;

/// <summary>
/// General extension methods and shorthand.
/// </summary>
public static class Extensions {

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
            if ( I > En ) { yield break; }
            if ( I < St ) { continue; }
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
    public static IEnumerable<T> Iterate<T>(this IEnumerator<T> Enum ) {
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
    public static string Join( this IEnumerable<string> Text, string Separator ) => string.Join(Separator, Text);

    /// <summary>
    /// Joins the strings into a single whole string.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <returns>The concatenation of all the specified strings.</returns>
    public static string Join( this IEnumerable<string> Text ) => string.Join(string.Empty, Text);

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
        Right = End >= L ? null : Text[(End+1)..];
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
        if ( Index < 0 || Index >= Ln || Length == 0 || En > Ln ) { return null; }
        return Text[Index..En];
    }
}
