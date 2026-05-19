using NMoneys;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Application.Commerce.Factories;

public static class OrderLineEntityFactory
{
    public static OrderLineEntity FromSellableInformation(SellableItemEntity sellableItem,
        SellableItemPriceEntity sellableItemPrice, string userID)
    {
        return new OrderLineEntity()
        {
            SellableItemID = sellableItem.SellableItemID,
            NameSnapshot = sellableItem.Name,
            Total = sellableItemPrice.Amount,
            UnitPrice = sellableItemPrice.Amount,
            Type = SellableItemType.Product,
            OrderLineID = Guid.NewGuid().ToString(),
            DiscountAmount = Money.Zero(),
            PaymentGatewayPriceIDSnapshot = sellableItemPrice.ExternalPriceID,
            PaymentGatewayProductIDSnapshot = sellableItemPrice.ExternalProductID,
            TaxAmount = Money.Zero(),
            SellablePriceID = Guid.NewGuid().ToString(),
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = userID,
            LastModifiedBy = userID,
        };
    }

}
