using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Commerce.Commands.CreateOrder;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Web.Catalog.Orders.Controllers;

namespace VibraHeka.Web.Controllers.Commerce;

public sealed class OrdersController(IMediator mediator, OrdersMapper mapper, ILogger<OrdersController> logger) : IOrdersController
{
    
    public override async Task<ActionResult<CreateOrderResponse>> CreateOrder(CreateOrderRequest body, CancellationToken cancellationToken = default)
    {
        CreateOrderDTO dto = mapper.ToDTO(body.Order);
        CreateOrderCommand command = new(dto);
        Result<OrderCheckoutModel> result = await mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            logger.LogInformation("Order created successfully");
            return Ok(mapper.ToResponse(result.Value));
        }
        logger.LogWarning("Order creation failed with error {Error}", result.Error);
        return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
    }
}
