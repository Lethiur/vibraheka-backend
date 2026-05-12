using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface ISellableItemPort
{
    public Task<Result<SellableItemEntity>> GetSellableItemByReferenceAsync(string referenceID, CancellationToken cancellationToken);
}
