using System.Diagnostics.CodeAnalysis;

namespace QGen.Lib.Common;

public sealed class Result : IResult {

    /// <summary>
    /// Initialises a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="Success">Whether the result is a success.</param>
    /// <param name="Message">The related diagnostic message.</param>
    public Result( bool Success, string Message ) {
        this.Success = Success;
        this.Message = Message;
    }

    /// <summary>
    /// The default message for successful results.
    /// </summary>
    /// <remarks>The message reads: "The event was successful."</remarks>
    internal const string MsgSuccess = "The event was successful.";

    /// <summary>
    /// The default message for unsuccessful results.
    /// </summary>
    /// <remarks>The message reads: "An unknown error occurred."</remarks>
    internal const string MsgError = "An unknown error occurred.";

    /// <summary>
    /// Initialises a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="Success">Whether the result is a success. If <see langword="true"/>, the message will read 'The event was successful.' If <see langword="false"/>, the message will read 'An unknown error occurred.'</param>
    public Result( bool Success ) : this(Success, Success ? MsgSuccess : MsgError) { }

    /// <summary>
    /// Initialises a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="Message">The related diagnostic message. If <see langword="null"/> or <see cref="string.Empty">empty</see>, the result is assumed successful (and the message will become 'The event was successful.'); otherwise if any value is provided, the result is assumed unsuccessful.</param>
    public Result( string? Message ) : this(Message is null, Message ?? MsgSuccess) { }

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
    public static implicit operator bool( Result Result ) => Result.Success;

    /// <summary>
    /// Performs an explicit conversion from <see cref="Result"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="Result">The result to convert.</param>
    /// <returns>The related diagnostic message.
    /// </returns>
    public static explicit operator string(Result Result) => Result.Message;

    /// <summary>
    /// Performs an explicit conversion from <see cref="bool"/> to <see cref="Result"/>.
    /// </summary>
    /// <param name="Success">Whether the result was successful.</param>
    /// <returns>
    /// A new <see cref="Result"/> instance utilising the '<see langword="new"/> <see cref="Result(bool)"/>' constructor.
    /// </returns>
    public static explicit operator Result( bool Success ) => new Result(Success);

}

public sealed class Result<T> : IResult<T> {

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
    /// <returns>The related diagnostic message.
    /// </returns>
    public static explicit operator string( Result<T> Result ) => Result.Message;

    /// <summary>
    /// Performs an explicit conversion from <typeparamref name="T"/> to <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="Value">The resultant value of the method execution. The execution result is assumed unsuccessful if the supplied value is <see langword="null"/>.</param>
    /// <returns>
    /// A new <see cref="Result{T}"/> instance utilising the '<see langword="new"/> <see cref="Result{T}(T?)"/>' constructor.
    /// </returns>
    public static explicit operator Result<T>( T? Value ) => new Result<T>(Value);

    #region Implementation of IResult<T>

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
}

/// <summary>
/// Represents the result of a method and its related diagnostic data.
/// </summary>
public interface IResult {

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the function was successful; otherwise, <see langword="false" />.
    /// </value>
    bool Success { get; }

    /// <summary>
    /// Gets the diagnostic message related to this message.
    /// </summary>
    /// <value>
    /// The diagnostic message.
    /// </value>
    string Message { get; }

    /// <summary>
    /// The default 'successful' result.
    /// </summary>
    public static readonly Result Successful = new Result(true);

    /// <summary>
    /// The default 'unexpected error' result.
    /// </summary>
    public static readonly Result UnexpectedError = new Result(false);

}

/// <summary>
/// Represents the result of a method and its related diagnostic data.
/// </summary>
/// <typeparam name="T">The resultant value type.</typeparam>
public interface IResult<T> : IResult {

    /// <summary>
    /// Gets the resultant value.
    /// </summary>
    /// <value>
    /// The value of the result.
    /// </value>
    /// <see cref="InvalidOperationException">The result value was attempted to be retrieved when the method execution was unsuccessful. <see cref="Value"/> can only be retrieved when <see cref="IResult.Success"/> is <see langword="true"/>.</see>
    T Value { get; }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required.
    //'new' keyword IS required as it hides a static field in a derived interface.

    /// <summary>
    /// The default 'successful' result.
    /// </summary>
    /// <remarks>The resultant value is equivalent to <see langword="default"/>(<see cref="Result{T}"/>).</remarks>
    public new static readonly Result<T> Successful = new Result<T>(true, Result.MsgSuccess, default!);

    /// <summary>
    /// The default 'unexpected error' result.
    /// </summary>
    public new static readonly Result<T> UnexpectedError = new Result<T>(false, Result.MsgError, default!);
#pragma warning restore CS0109 // Member does not hide an inherited member; new keyword is not required.
#pragma warning restore IDE0079 // Remove unnecessary suppression

    /// <summary>
    /// Attempts to get the resultant value of the method, returning <see langword="true"/> if the result was a success.
    /// </summary>
    /// <param name="Value">The resultant value.</param>
    /// <returns><see langword="true"/> if the result was a success; otherwise <see langword="false"/>.</returns>
    public bool TryGetResult( [NotNullWhen(true)] out T? Value ) {
        if ( Success ) {
            Value = this.Value;
#pragma warning disable CS8762
            return true;
#pragma warning restore CS8762
        }
        Value = default;
        return false;
    }

}