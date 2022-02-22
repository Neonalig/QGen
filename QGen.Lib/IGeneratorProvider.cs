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
public interface IGeneratorProvider {

    /// <summary>
    /// Asynchronously retrieves the relevant generators from this interface implementation.
    /// </summary>
    /// <param name="Token">The cancellation token.</param>
    /// <returns>The collection of generators to use.</returns>
    Task<Result<IEnumerable<IFileGenerator>>> GetGeneratorsAsync( CancellationToken Token = new CancellationToken() );

}
