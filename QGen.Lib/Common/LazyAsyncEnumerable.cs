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

#endregion

namespace QGen.Lib.Common;

#pragma warning disable CS8774 // Member must have a non-null value when exiting.

/// <summary>
/// Asynchronous equivalent to <see cref="Lazy{T}"/>.
/// </summary>
/// <typeparam name="T">The collection containing type.</typeparam>
public class LazyAsyncEnumerable<T> : IAsyncEnumerable<T>, IEnumerable<T> {
    readonly IAsyncEnumerable<T> _Enum;
    readonly Func<CancellationToken, IAsyncEnumerable<T>> _EnumCt;

    ReadOnlyCollection<T>? _Values;

    /// <summary>
    /// Gets the collection of values.
    /// </summary>
    /// <value>
    /// The collection of values.
    /// </value>
    /// <exception cref="InvalidOperationException"><see cref="GetValuesAsync(CancellationToken)"/> must be invoked before the value can be retrieved. Check via the '<see cref="HasValues"/>' property whether the value is already instantiated or not.</exception>
    public ReadOnlyCollection<T> Values {
        get {
            if ( !HasValues ) {
                throw new InvalidOperationException("GetValuesAsync() must be invoked before the value can be retrieved. Check via the 'HasValues' property.");
            }
            return _Values!;
        }
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="Values">The collection of values.</param>
    public LazyAsyncEnumerable( ReadOnlyCollection<T> Values ) {
        _Enum = null!;
        _EnumCt = null!;
        _Values = Values;
        HasValues = true;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="EnumTask">The task which asynchronously constructs/retrieves the collection of values.</param>
    public LazyAsyncEnumerable( Task<IEnumerable<T>> EnumTask ) : this(EnumTask.GetAsyncEnumerable()) { }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="EnumTaskFunc">The task which asynchronously constructs/retrieves the collection of values.</param>
    public LazyAsyncEnumerable( Func<Task<IEnumerable<T>>> EnumTaskFunc ) : this(EnumTaskFunc.Transform(Extensions.GetAsyncEnumerable)) { }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="Enum">The task which asynchronously constructs/retrieves the collection of values.</param>
    public LazyAsyncEnumerable( IAsyncEnumerable<T> Enum ) {
        _Enum = Enum;
        _EnumCt = null!;
        _Values = null;
        HasValues = false;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="EnumFunc">The task which asynchronously constructs/retrieves the collection of values.</param>
    public LazyAsyncEnumerable( Func<IAsyncEnumerable<T>> EnumFunc ) {
        _Enum = EnumFunc();
        _EnumCt = null!;
        _Values = null;
        HasValues = false;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsyncEnumerable{T}"/> class.
    /// </summary>
    /// <param name="EnumFunc">The task which asynchronously constructs/retrieves the collection of values.</param>
    public LazyAsyncEnumerable( Func<CancellationToken, IAsyncEnumerable<T>> EnumFunc ) {
        _EnumCt = EnumFunc;
        _Enum = null!;
        _Values = null;
        HasValues = false;
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="Values"/> collection has been assigned yet.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if <see cref="ConstructAsync(CancellationToken)"/> or <see cref="GetValuesAsync(CancellationToken)"/> has been invoked before; otherwise, <see langword="false" />.
    /// </value>
    public bool HasValues { get; private set; }

    /// <summary>
    /// Asynchronously constructs/retrieves a reference for the <see cref="Values"/>.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    [MemberNotNull(nameof(_Values))]
    public async Task ConstructAsync( CancellationToken Token ) {
        if ( HasValues ) { return; }
        _Values = (await (_EnumCt is not null ? _EnumCt(Token) : _Enum).IterateAsync(Token)).AsReadOnly();
        HasValues = true;
    }

    /// <summary>
    /// Asynchronously constructs/retrieves a reference for the <see cref="Values"/>.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>A task which asynchronously retrieves and caches the values in the asynchronous collection.</returns>
    [MemberNotNull(nameof(_Values))]
    public async Task<ReadOnlyCollection<T>> GetValuesAsync( CancellationToken Token ) {
        await ConstructAsync(Token);
        return _Values!;
    }

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="LazyAsyncEnumerable{T}"/> to <see cref="T"/>.
    /// </summary>
    /// <param name="As">As.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator ReadOnlyCollection<T>( LazyAsyncEnumerable<T> As ) => As.Values;

    /// <summary>
    /// Performs an explicit conversion from <see cref="T"/> to <see cref="LazyAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="Vals">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator LazyAsyncEnumerable<T>( ReadOnlyCollection<T> Vals ) => new LazyAsyncEnumerable<T>(Vals);

    /// <inheritdoc />
    public async IAsyncEnumerator<T> GetAsyncEnumerator( CancellationToken Token = default ) {
        foreach ( T Item in await GetValuesAsync(Token) ) {
            yield return Item;
        }
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() {
        if ( !HasValues ) {
            throw new InvalidOperationException("GetValuesAsync() must be invoked before the value can be retrieved. Check via the 'HasValues' property.");
        }
        return _Values!.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the awaiter.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The task awaiter.</returns>
    public TaskAwaiter<ReadOnlyCollection<T>> GetAwaiter( CancellationToken Token ) => GetValuesAsync(Token).GetAwaiter();

    /// <inheritdoc cref="GetAwaiter(CancellationToken)"/>
    public TaskAwaiter<ReadOnlyCollection<T>> GetAwaiter() => GetAwaiter(new CancellationToken());
}