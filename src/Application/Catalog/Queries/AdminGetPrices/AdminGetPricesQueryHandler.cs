using CSharpFunctionalExtensions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Application.Catalog.Queries.AdminGetPrices;

public class AdminGetPricesQueryHandler(ISellableItemPort sellableItemPort)
    : IRequestHandler<AdminGetPrices, Result<SellableItemEntity>>
{
    public async Task<Result<SellableItemEntity>> Handle(AdminGetPrices request, CancellationToken cancellationToken)
    {
        return await sellableItemPort
            .GetSellableItemByReferenceAsync(request.RefID, cancellationToken);
    }
}
