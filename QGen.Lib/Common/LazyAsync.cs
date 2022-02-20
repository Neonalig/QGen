#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Diagnostics.CodeAnalysis;

#endregion

namespace QGen.Lib.Common;

#pragma warning disable CS8774 // Member must have a non-null value when exiting.

/// <summary>
/// Asynchronous equivalent to <see cref="Lazy{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class LazyAsync<T> {
    readonly Task<T>? _Task;
    readonly Func<CancellationToken, Task<T>>? _TaskCt;

    T? _Value;

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    /// <exception cref="InvalidOperationException"><see cref="GetValueAsync()"/> must be invoked before the value can be retrieved. Check via the '<see cref="HasValue"/>' property whether the value is already instantiated or not.</exception>
    public T Value {
        get {
            if ( !HasValue ) {
                throw new InvalidOperationException("GetValueAsync() must be invoked before the value can be retrieved. Check via the 'HasValue' property.");
            }
            return _Value!;
        }
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsync{T}"/> class.
    /// </summary>
    /// <param name="Value">The value.</param>
    public LazyAsync( T Value ) {
        _Task = null!;
        _Value = Value;
        HasValue = true;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsync{T}"/> class.
    /// </summary>
    /// <param name="Task">The task which asynchronously constructs/retrieves the value.</param>
    public LazyAsync( Task<T> Task ) {
        _Task = Task;
        _Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsync{T}"/> class.
    /// </summary>
    /// <param name="TaskFunc">The task which asynchronously constructs/retrieves the value.</param>
    public LazyAsync( Func<Task<T>> TaskFunc ) : this(TaskFunc()) { }

    /// <summary>
    /// Initialises a new instance of the <see cref="LazyAsync{T}"/> class.
    /// </summary>
    /// <param name="TaskFunc">The task which asynchronously constructs/retrieves the value. Accepts a <see cref="CancellationToken"/>.</param>
    public LazyAsync( Func<CancellationToken, Task<T>> TaskFunc ) {
        _TaskCt = TaskFunc;
        _Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Gets a value indicating whether a <see cref="Value"/> has been assigned yet.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if <see cref="ConstructAsync()"/> or <see cref="GetValueAsync()"/> has been invoked before; otherwise, <see langword="false" />.
    /// </value>
    public bool HasValue { get; private set; }

    /// <inheritdoc cref="ConstructAsync(CancellationToken)"/>
    [MemberNotNull(nameof(_Value))]
    public async Task ConstructAsync() {
        if ( HasValue ) { return; }
        if ( _Task is null ) {
            _Value = await _TaskCt!(CancellationToken.None);
        } else {
            _Value = await _Task;
        }
        HasValue = true;
    }

    /// <summary>
    /// Asynchronously constructs/retrieves a reference for the <see cref="Value"/>.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    [MemberNotNull(nameof(_Value))]
    public async Task ConstructAsync( CancellationToken Token ) {
        if ( HasValue ) { return; }
        if ( _TaskCt is null ) {
            _Value = await _Task!;
        } else {
            _Value = await _TaskCt(Token);
        }
        HasValue = true;
    }

    /// <inheritdoc cref="GetValueAsync(CancellationToken)"/>
    [MemberNotNull(nameof(_Value))]
    public async Task<T> GetValueAsync() {
        await ConstructAsync();
        return _Value;
    }

    /// <summary>
    /// Asynchronously constructs/retrieves a reference for the <see cref="Value"/>.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The constructed/retrieved <see cref="Value"/>.</returns>
    [MemberNotNull(nameof(_Value))]
    public async Task<T> GetValueAsync( CancellationToken Token ) {
        await ConstructAsync(Token);
        return _Value;
    }

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="LazyAsync{T}"/> to <see cref="T"/>.
    /// </summary>
    /// <param name="As">As.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator T( LazyAsync<T> As ) => As.Value;

    /// <summary>
    /// Performs an explicit conversion from <see cref="T"/> to <see cref="LazyAsync{T}"/>.
    /// </summary>
    /// <param name="Val">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator LazyAsync<T>( T Val ) => new LazyAsync<T>(Val);

    /// <summary>
    /// Performs an explicit conversion from <see cref="Task{T}"/> to <see cref="LazyAsync{T}"/>.
    /// </summary>
    /// <param name="Task">The task which asynchronously constructs/retrieves the value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator LazyAsync<T>( Task<T> Task ) => new LazyAsync<T>(Task);

    /// <summary>
    /// Performs an explicit conversion from <see cref="Func{T}"/> to <see cref="LazyAsync{T}"/>.
    /// </summary>
    /// <param name="TaskFunc">The task which asynchronously constructs/retrieves the value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator LazyAsync<T>( Func<Task<T>> TaskFunc ) => new LazyAsync<T>(TaskFunc);

    /// <summary>
    /// Performs an explicit conversion from <see cref="Func{CancellationToken, Task}"/> to <see cref="LazyAsync{T}"/>.
    /// </summary>
    /// <param name="TaskFunc">The task which asynchronously constructs/retrieves the value. Accepts a <see cref="CancellationToken"/>.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static explicit operator LazyAsync<T>( Func<CancellationToken, Task<T>> TaskFunc ) => new LazyAsync<T>(TaskFunc);
}