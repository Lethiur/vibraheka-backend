using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.Catalog.Models;

public class SellableInformationModel
{
    public SellableItemPriceEntity Price { get; set; } = default!;
    public SellableItemEntity Item { get; set; } = default!;
}
