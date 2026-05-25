using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Models;

/// <summary>
/// DTO for SellableItemPriceEntity — uses primitive types to avoid NMoneys.Money
/// circular-reference issue during JSON serialization.
/// </summary>
public record SellableItemPriceDto(
    string SellableItemPriceID,
    string SellableItemID,
    decimal Amount,
    string CurrencyCode,
    PriceKind Kind,
    BillingInterval? BillingInterval,
    string ExternalProductID,
    string ExternalPriceID,
    bool IsActive)
{
    public static SellableItemPriceDto FromDomain(SellableItemPriceEntity entity) =>
        new(
            entity.SellableItemPriceID,
            entity.SellableItemID,
            entity.Amount.Amount,
            entity.Amount.CurrencyCode.ToString(),
            entity.Kind,
            entity.BillingInterval,
            entity.ExternalProductID,
            entity.ExternalPriceID,
            entity.IsActive);
}

