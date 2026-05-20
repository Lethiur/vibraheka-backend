using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Web.Entities;
namespace VibraHeka.Web.Mappers;
/// <summary>Maps Web request DTOs to Application-layer DTOs for the Commerce/Orders feature.</summary>
[Mapper]
public partial class OrderRequestMapper
{
    public partial CreateOrderDTO ToDto(CreateOrderRequest request);
    public partial CreateOrderLineDTO ToDto(CreateOrderLineRequest request);
}
