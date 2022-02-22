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
/// <typeparam name="T">The resultant value type.</typeparam>
public interface IResult<out T> : IResult {

    /// <summary>
    /// Gets the resultant value.
    /// </summary>
    /// <value>
    /// The value of the result.
    /// </value>
    /// <see cref="InvalidOperationException">The result value was attempted to be retrieved when the method execution was unsuccessful. <see cref="Value"/> can only be retrieved when <see cref="IResult.Success"/> is <see langword="true"/>.</see>
    T Value { get; }

}