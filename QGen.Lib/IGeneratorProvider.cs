#region Copyright (C) 2017-2022  Cody Bock
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using QGen.Lib.Common;

#endregion

namespace QGen.Lib;

/// <summary>
/// Defines a class which provides various <see cref="IFileGenerator"/> types based on a particular pre-chosen criteria. (i.e. active folder)
/// </summary>
/// <remarks>The deriving class <b>must</b> define a default, parameterless constructor.</remarks>
public interface IGeneratorProvider {

    /// <summary>
    /// Asynchronously retrieves the relevant generators from this interface implementation.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The collection of generators to use.</returns>
    Task<Result<IEnumerable<IFileGenerator>>> GetGeneratorsAsync( CancellationToken Token = new CancellationToken() );

    /// <summary>
    /// Gets the requested root folder's relative path.
    /// <para/><b>Example: </b>
    /// <br/><example>
    /// <c>SampleProject/Scripts/Helpers/</c>
    /// </example>
    /// </summary>
    /// <value>
    /// The relative path to the root folder.
    /// </value>
    string RequestedRootFolder { get; }

    /// <summary>
    /// Gets the root folder's absolute path as is default on most systems.
    /// <para/><b>Example: </b>
    /// <br/><example>
    /// <c>%userprofile%/Documents/Unity/SampleProject/Scripts/Helpers/</c>
    /// </example>
    /// </summary>
    /// <value>
    /// The root folder's absolute path, or <see langword="null"/> if there is no general default.
    /// </value>
    string? DefaultRootFolder { get; }

}
