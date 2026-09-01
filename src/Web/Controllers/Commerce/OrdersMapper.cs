using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Web.Catalog.Orders.Controllers;

namespace VibraHeka.Web.Controllers.Commerce;

[Mapper]
public partial class OrdersMapper
{
    [MapProperty(nameof(OrderDTO.Lines), nameof(CreateOrderDTO.OrderLines))]
    public partial CreateOrderDTO ToDTO(OrderDTO dto);
    
    [MapProperty(nameof(OrderLineDTO.ProductId), nameof(CreateOrderLineDTO.SellableItemID))]
    [MapProperty(nameof(OrderLineDTO.PriceId), nameof(CreateOrderLineDTO.SellableItemPriceID))]
    private partial CreateOrderLineDTO ToDTO(OrderLineDTO dto);
    
    [MapProperty(nameof(OrderCheckoutModel.CheckoutURL), nameof(CreateOrderResponse.CheckoutUrl))]
    [MapProperty(nameof(OrderCheckoutModel.ExpiresAtUTC), nameof(CreateOrderResponse.ExpiresAt))]
    public partial CreateOrderResponse ToResponse(OrderCheckoutModel model);
}
