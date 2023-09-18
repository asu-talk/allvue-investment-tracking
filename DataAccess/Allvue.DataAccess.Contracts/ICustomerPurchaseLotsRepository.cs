using Allvue.DataAccess.Contracts.Dtos;

namespace Allvue.DataAccess.Contracts;

/// TODO: design and specify repository related exceptions
/// <summary>
/// Implementation should be thread-safe
/// </summary>
public interface ICustomerPurchaseLotsRepository
{
    Task WritePurchaseLotAsync(LotDto purchaseLot, CancellationToken cancellationToken);

    /// <summary>
    /// Returns purchase lots in order they've been written
    /// Note: usually repository will have additional methods allowing to read entries using some filtering/sorting criteria
    /// </summary>
    Task<IReadOnlyCollection<LotDto>> ReadOrderedPurchaseLotsAsync(CancellationToken cancellationToken);
}
