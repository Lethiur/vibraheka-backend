namespace VibraHeka.Web.Entities;
/// <summary>
/// HTTP request payload for creating a new order (POST api/v1/orders).
/// </summary>
public class CreateOrderRequest
{
    /// <summary>One or more order lines to include in the order.</summary>
    public IReadOnlyList<CreateOrderLineRequest> OrderLines { get; set; } = Array.Empty<CreateOrderLineRequest>();
    /// <summary>Optional promotion / discount code.</summary>
    public string PromotionCode { get; set; } = string.Empty;
    /// <summary>Client-supplied idempotency key to prevent duplicate orders.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}
