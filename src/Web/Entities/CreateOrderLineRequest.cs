namespace VibraHeka.Web.Entities;
/// <summary>
/// A single line item within a <see cref="CreateOrderRequest"/>.
/// </summary>
public class CreateOrderLineRequest
{
    /// <summary>Identifier of the sellable item to purchase.</summary>
    public string SellableItemID { get; set; } = string.Empty;
    /// <summary>Identifier of the price variant to use for the sellable item.</summary>
    public string SellableItemPriceID { get; set; } = string.Empty;
    /// <summary>Number of units to purchase. Must be greater than zero.</summary>
    public int Quantity { get; set; }
}
