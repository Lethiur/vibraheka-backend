using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Application.Catalog.Commands.AdminCreatePrice;

public class AdminCreatePriceCommandHandler(ISellableItemPricePort sellableItemPricePort) : IRequestHandler<AdminCreatePriceCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AdminCreatePriceCommand request, CancellationToken cancellationToken)
    {
        
        
        var result = await sellableItemPricePort.CreateSellableItemPriceAsync(
            request.SellableItemID, request.Price, request.Currency, cancellationToken);

        return result;
    }
    
}
