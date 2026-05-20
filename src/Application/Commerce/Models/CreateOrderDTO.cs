namespace VibraHeka.Application.Commerce.Models;

public class CreateOrderDTO
{
    public IReadOnlyList<CreateOrderLineDTO> OrderLines { get; set; } = Array.Empty<CreateOrderLineDTO>();
    public string PromotionCode { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
}
