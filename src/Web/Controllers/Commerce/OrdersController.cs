using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Commerce.Commands.CreateOrder;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Domain.Common.Errors;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;
using VibraHeka.Web.Mappers;

namespace VibraHeka.Web.Controllers.Commerce;

[ApiController]
[Route("api/v1/orders")]
public sealed class OrdersController(IMediator mediator, OrderRequestMapper mapper, ILogger<OrdersController> logger)
{
    [HttpPost]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        CreateOrderDTO dto = mapper.ToDto(request);
        CreateOrderCommand command = new(dto);
        Result<CreateOrderResponse> result = await mediator.Send(command, ct);
        if (result.IsSuccess)
        {
            logger.LogInformation("Order created successfully");
            return new ObjectResult(ResponseEntity.FromSuccess(result.Value)) { StatusCode = StatusCodes.Status201Created };
        }
        logger.LogWarning("Order creation failed with error {Error}", result.Error);
        return result.Error switch
        {
            CommerceErrors.OrderPlacementFailed => new ConflictObjectResult(ResponseEntity.FromError(result.Error)),
            DomainErrors.GenericError => new ObjectResult(ResponseEntity.FromError(result.Error)) { StatusCode = StatusCodes.Status500InternalServerError },
            _ => new BadRequestObjectResult(ResponseEntity.FromError(result.Error))
        };
    }
}
