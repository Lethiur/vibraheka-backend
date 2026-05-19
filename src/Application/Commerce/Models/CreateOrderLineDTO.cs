namespace VibraHeka.Application.Commerce.Models;

public class CreateOrderLineDTO
{
    public string SellableItemID { get; set; } = string.Empty;
    public string SellableItemPriceID { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
}
