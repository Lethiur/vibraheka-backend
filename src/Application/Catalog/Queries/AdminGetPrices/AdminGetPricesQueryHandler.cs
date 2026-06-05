using CSharpFunctionalExtensions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Application.Catalog.Queries.AdminGetPrices;

public class AdminGetPricesQueryHandler(ISellableItemPort sellableItemPort)
    : IRequestHandler<AdminGetPrices, Result<SellableItemDto>>
{
    public async Task<Result<SellableItemDto>> Handle(AdminGetPrices request, CancellationToken cancellationToken)
    {
        return await sellableItemPort
            .GetSellableItemByReferenceAsync(request.RefID, cancellationToken)
            .Map(SellableItemDto.FromDomain);
    }
}
