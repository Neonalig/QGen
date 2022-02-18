#region Copyright (C) 2017-2022  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

namespace QGen.Extensions;

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
}
