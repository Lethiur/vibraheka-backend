using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Models;

/// <summary>
/// DTO for SellableItemEntity — safe for JSON serialization; contains no domain value objects
/// with recursive properties.
/// </summary>
public record SellableItemDto(
    string SellableItemID,
    SellableItemType Type,
    string ReferenceID,
    string Name,
    bool IsActive,
    IReadOnlyList<SellableItemPriceDto> Prices)
{
    public static SellableItemDto FromDomain(SellableItemEntity entity) =>
        new(
            entity.SellableItemID,
            entity.Type,
            entity.ReferenceID,
            entity.Name,
            entity.IsActive,
            entity.Prices.Select(SellableItemPriceDto.FromDomain).ToList());
}

