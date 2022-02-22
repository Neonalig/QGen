using QGen.Lib.Common;

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
    Task<Result<IEnumerable<IFileGenerator>>> GetGeneratorsAsync( CancellationToken Token = new() );

}
