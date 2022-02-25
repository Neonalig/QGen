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

public sealed class Result : IResult {

#if DEBUG
    /// <inheritdoc />
    public object? DbgVal => null;
#endif

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
    /// Performs an <see langword="implicit"/> conversion from <see cref="Exception"/> to <see cref="Result"/>.
    /// </summary>
    /// <param name="Ex">The exception.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator Result( Exception Ex ) => new Result(false, $"[{Ex.HResult}] {Ex}");

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

    static readonly Result _CancelledResult = new Result(false, "The task was cancelled.");

    /// <summary>
    /// The default 'successful' result.
    /// </summary>
    public static readonly Result Successful = new Result(true);

    /// <summary>
    /// The default 'unexpected error' result.
    /// </summary>
    public static readonly Result UnexpectedError = new Result(false);

    /// <summary>
    /// The default result to return when <see cref="CancellationToken.IsCancellationRequested"/> becomes <see langword="true"/>.
    /// </summary>
    public static Result Cancelled() => _CancelledResult;

    /// <summary>
    /// The result to return when <see cref="CancellationToken.IsCancellationRequested"/> becomes <see langword="true"/>.
    /// </summary>
    /// <param name="Verbose">If <see langword="true"/>, the name of the calling member (task method name) is included in the diagnostics message.</param>
    /// <param name="CallerMemberName">The name of the calling member.</param>
    public static Result Cancelled( bool Verbose, [CallerMemberName] string CallerMemberName = "" ) => Verbose ? new Result(false, $"The task '{CallerMemberName}' was cancelled.") : _CancelledResult;

    /// <summary>
    /// An unsuccessful result related to a file path being invalid.
    /// </summary>
    /// <param name="Path">The requested path to the file.</param>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static Result FilePathInvalid( string Path ) => new Result(false, $"The file path '{Path}' was invalid and could not be resolved.");

    /// <summary>
    /// An unsuccessful result related to a file path not being found.
    /// </summary>
    /// <param name="Path">The requested path to the file.</param>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static Result FileNotFound( string Path ) => new Result(false, $"The file with the path '{Path}' could not be found.");

    /// <summary>
    /// An unsuccessful result related to a directory path being invalid.
    /// </summary>
    /// <param name="Path">The requested path to the directory.</param>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static Result DirectoryPathInvalid( string Path ) => new Result(false, $"The directory path '{Path}' was invalid and could not be resolved.");

    /// <summary>
    /// An unsuccessful result related to a directory path not being found.
    /// </summary>
    /// <param name="Path">The requested path to the directory.</param>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static Result DirectoryNotFound( string Path ) => new Result(false, $"The directory with the path '{Path}' could not be found.");

    /// <summary>
    /// An unsuccessful result related to a <see cref="IFileGenerator"/> lookup failing.
    /// </summary>
    /// <param name="Generator">The source generator that failed.</param>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static Result LookupFailed( IFileGenerator Generator ) => new Result(false, $"Attempted file lookup on the source generator '{Generator.Name}' (v{Generator.Version}) failed.");

    /// <summary>
    /// An unsuccessful result related to a <see cref="Type"/> not containing a default, parameterless constructor.
    /// </summary>
    /// <param name="Tp">The type that does not provide a default, parameterless constructor.</param>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static Result MissingParameterlessConstructor( Type Tp ) => new Result(false, $"The type '{Tp.FullName}' derives from type {nameof(IGeneratorProvider)}, but does not define a default, parameterless constructor.");

    /// <summary>
    /// An unsuccessful result related to a dialog being closed prematurely by the user.
    /// </summary>
    /// <returns>A new <see cref="Result"/> instance.</returns>
    public static readonly Result UserCancelledDialog = new Result(false, "The user cancelled the dialog.");

    /// <summary>
    /// Logs the current result to the debug trace listeners.
    /// </summary>
    [Conditional("DEBUG")]
    public void Log() => Debug.WriteLine($"The method executed with the message: \"{Message}\".", Success ? "SUCCESS" : "ERROR");

    /// <summary>
    /// Logs the current result to the debug trace listeners.
    /// </summary>
    /// <param name="WhenSuccess">If <see langword="false"/>, successful results are <b>not</b> logged; otherwise all results are logged regardless of being successful or not.</param>
    [Conditional("DEBUG")]
    public void Log( bool WhenSuccess ) {
        if ( !Success || WhenSuccess ) {
            Log();
        }
    }

}