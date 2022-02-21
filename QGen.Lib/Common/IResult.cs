#region Copyright (C) 2017-2022  Cody Bock

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html

#endregion

namespace QGen.Lib.Common;

/// <summary>
/// Represents the result of a method and its related diagnostic data.
/// </summary>
public interface IResult {
#if DEBUG    
    /// <inheritdoc cref="IResult{T}.Value"/>
    /// <remarks>Used exclusively for <see cref="Extensions.Log(IResult)"/> in DEBUG builds.</remarks>
    object? Val { get; }
#endif

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

}