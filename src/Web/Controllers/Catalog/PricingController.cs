using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Commands.AdminActivatePrice;
using VibraHeka.Application.Catalog.Commands.AdminCreatePrice;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Queries.AdminGetPrices;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Controllers.Catalog;

/// <summary>
/// Handles HTTP requests related to pricing operations within the catalog.
/// </summary>
[ApiController]
[Route("api/v1/catalog/prices")]
public class PricingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves pricing details for a specified reference ID.
    /// </summary>
    /// <param name="refID">The unique reference ID used to query pricing details.</param>
    /// <param name="ct">The cancellation token used to signal request cancellation.</param>
    /// <returns>A response containing a successful result with a SellableItemDto or an error code.</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPrices(
        [FromQuery(Name = "RefID")] string refID,
        CancellationToken ct)
    {
        AdminGetPrices query = new(refID);

        Result<SellableItemDto> result = await mediator.Send(query, ct);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePrice([FromBody] CreateProductPriceRequest request, CancellationToken ct)
    {
        Result<string> commandResult = await mediator.Send(new AdminCreatePriceCommand(request.SellableItemID, request.Amount, request.Currency, true, request.BillingInterval), ct);

        if (commandResult.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(commandResult.Error));
        } 

        return new OkObjectResult(ResponseEntity.FromSuccess(commandResult.Value));
    }

    [HttpPost("activate")]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivatePrice([FromBody] ActivateProductPriceRequest request, CancellationToken ct)
    {
        Result<Unit> commandResult = await mediator.Send(new AdminActivatePriceCommand(request.SellableItemPriceID, request.SellableItemID), ct);

        if (commandResult.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(commandResult.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(commandResult.Value));
    }
}
