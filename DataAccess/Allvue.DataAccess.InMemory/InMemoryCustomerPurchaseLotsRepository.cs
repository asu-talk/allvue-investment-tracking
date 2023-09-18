using System.Collections.Concurrent;
using Allvue.DataAccess.Contracts;
using Allvue.DataAccess.Contracts.Dtos;

namespace Allvue.DataAccess.InMemory;

// Note: usually implementation is 'internal' and registered in DI as module (Autofac for example) or in some other way.
public class InMemoryCustomerPurchaseLotsRepository : ICustomerPurchaseLotsRepository
{
    private readonly ConcurrentQueue<LotDto> _purchaseLots = new();

    public async Task WritePurchaseLotAsync(LotDto purchaseLot, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _purchaseLots.Enqueue(purchaseLot);
    }

    public async Task<IReadOnlyCollection<LotDto>> ReadOrderedPurchaseLotsAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var result = _purchaseLots.ToArray();
        return result;
    }
}
