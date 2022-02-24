#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Runtime.CompilerServices;

#endregion

namespace QGen.Lib.Common;

public readonly struct Result<T> : IResult<T> {

#if DEBUG
    /// <inheritdoc />
    public object? Val => Value;
#endif

    /// <summary>
    /// Initialises a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="Success">Whether the result is a success.</param>
    /// <param name="Message">The related diagnostic message.</param>
    /// <param name="Value">The resultant value of the method execution.</param>
    public Result( bool Success, string Message, T Value ) {
        this.Success = Success;
        this.Message = Message;
        _Value = Value;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="Value">The resultant value of the method execution. The execution result is assumed unsuccessful if the supplied value is <see langword="null"/>.</param>
    public Result( T? Value ) : this(Value is not null, Value is not null ? Result.MsgSuccess : Result.MsgError, Value!) { }

    #region Implementation of IResult

    /// <inheritdoc />
    public bool Success { get; }

    /// <inheritdoc />
    public string Message { get; }

    #endregion

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="Result"/> to <see cref="bool"/>.
    /// </summary>
    /// <param name="Result">The result to convert.</param>
    /// <returns>
    /// Whether the result was successful.
    /// </returns>
    public static implicit operator bool( Result<T> Result ) => Result.Success;

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="Exception"/> to <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="Ex">The exception.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator Result<T>( Exception Ex ) => From(Ex);

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="Result"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="Result">The result to convert.</param>
    /// <returns>
    /// The resultant value of the method execution.
    /// </returns>
    /// <see cref="InvalidOperationException">The result value was attempted to be retrieved when the method execution was unsuccessful. <see cref="Value"/> can only be retrieved when <see cref="IResult.Success"/> is <see langword="true"/>.</see>
    [DebuggerHidden]
    public static implicit operator T( Result<T> Result ) => Result.Value;

    /// <summary>
    /// Performs an explicit conversion from <see cref="Result"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="Result">The result to convert.</param>
    /// <returns>
    /// The related diagnostic message.
    /// </returns>
    public static explicit operator string( Result<T> Result ) => Result.Message;

    /// <summary>
    /// Performs an explicit conversion from <typeparamref name="T"/> to <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="Value">The resultant value of the method execution. The execution result is assumed unsuccessful if the supplied value is <see langword="null"/>.</param>
    /// <returns>
    /// A new <see cref="Result{T}"/> instance utilising the '<see langword="new"/> <see cref="Result{T}(T?)"/>' constructor.
    /// </returns>
    public static implicit operator Result<T>( T? Value ) => new Result<T>(Value);

    /// <summary>
    /// Performs an <see langword="implicit"/> conversion from <see cref="Result{T}"/> to <see cref="Result"/>.
    /// </summary>
    /// <param name="Result">The result.</param>
    /// <returns>
    /// A new <see cref="Result"/> instance utilising the '<see langword="new"/> <see cref="Result(bool, string)"/>' constructor.
    /// </returns>
    public static implicit operator Result( Result<T> Result ) => new Result(Result.Success, Result.Message);

    /// <inheritdoc cref="From(T, bool, string)"/>
    public static Result<T> From( T Value ) => new Result<T>(Value);

    /// <inheritdoc cref="From(T, bool, string)"/>
    public static Result<T> From( T Value, bool Success ) => From(Value, Success, Success ? Result.MsgSuccess : Result.MsgError);

    /// <summary>
    /// Constructs a new unsuccessful result from the specified exception message.
    /// </summary>
    /// <param name="Ex">The exception.</param>
    /// <returns>A new unsuccessful result with the specified exception message.</returns>
    public static Result<T> From( Exception Ex ) => new Result<T>(false, $"[{Ex.HResult}] {Ex}", default!);

    /// <summary>
    /// Constructs a new result from the given value.
    /// </summary>
    /// <param name="Value">The value.</param>
    /// <param name="Success">Whether the result was a success.</param>
    /// <param name="Message">The related diagnostics message.</param>
    /// <returns>A new successful result with the specified value.</returns>
    public static Result<T> From( T Value, bool Success, string Message ) => new Result<T>(Success, Message, Value);

    #region IResult<T> Implementation

    readonly T _Value;

    /// <inheritdoc />
    public T Value {
        [DebuggerHidden]
        get {
            if ( !Success ) {
                throw new InvalidOperationException("The result value was attempted to be retrieved when the method execution was unsuccessful. The value can only be retrieved when Result<T>.Success is true.");
            }
            return _Value;
        }
    }

    #endregion

    /// <summary>
    /// The default 'successful' result.
    /// </summary>
    /// <remarks>The resultant value is equivalent to <see langword="default"/>(<see cref="Result{T}"/>).</remarks>
    public static readonly Result<T> Successful = new Result<T>(true, Result.MsgSuccess, default!);

    /// <summary>
    /// The default 'unexpected error' result.
    /// </summary>
    public static readonly Result<T> UnexpectedError = new Result<T>(false, Result.MsgError, default!);

    /// <inheritdoc cref="Result.Cancelled()"/>
    public static Result<T> Cancelled() => Result.Cancelled().ForceType<T>();

    /// <inheritdoc cref="Result.Cancelled(bool, string)"/>
    public static Result<T> Cancelled( bool Verbose, [CallerMemberName] string CallerMemberName = "" ) => Result.Cancelled(Verbose, CallerMemberName).ForceType<T>();

    /// <inheritdoc cref="Result.FilePathInvalid(string)"/>
    public static Result<T> FilePathInvalid( string Path ) => Result.FilePathInvalid(Path).ForceType<T>();

    /// <inheritdoc cref="Result.FileNotFound(string)"/>
    public static Result<T> FileNotFound( string Path ) => Result.FileNotFound(Path).ForceType<T>();

    /// <inheritdoc cref="Result.DirectoryPathInvalid(string)"/>
    public static Result<T> DirectoryPathInvalid( string Path ) => Result.DirectoryPathInvalid(Path).ForceType<T>();

    /// <inheritdoc cref="Result.DirectoryNotFound(string)"/>
    public static Result<T> DirectoryNotFound( string Path ) => Result.DirectoryNotFound(Path).ForceType<T>();

    /// <inheritdoc cref="Result.LookupFailed(IFileGenerator)"/>
    public static Result<T> LookupFailed( IFileGenerator Generator ) => Result.LookupFailed(Generator).ForceType<T>();

    /// <inheritdoc cref="Result.MissingParameterlessConstructor(Type)"/>
    public static Result<T> MissingParameterlessConstructor() => new Result<T>(false, $"The type '{typeof(T).FullName}' derives from type {nameof(IGeneratorProvider)}, but does not define a default, parameterless constructor.", default!);

    /// <inheritdoc cref="Result.MissingParameterlessConstructor(Type)"/>
    public static Result<T> MissingParameterlessConstructor(Type Tp) => Result.MissingParameterlessConstructor(Tp).ForceType<T>();

    /// <inheritdoc cref="Result.UserCancelledDialog"/>
    public static readonly Result<T> UserCancelledDialog = Result.UserCancelledDialog.ForceType<T>();

    /// <summary>
    /// Attempts to construct the result using the specified function, returning an unsuccessful result if an exception is thrown.
    /// </summary>
    /// <param name="Cto">The constructor method.</param>
    /// <returns>The result of the function.</returns>
    public static Result<T> TryCatch( Func<T> Cto ) {
        try {
            return Cto.Invoke();
        } catch (Exception Ex ) {
            return Ex;
        }
    }

}