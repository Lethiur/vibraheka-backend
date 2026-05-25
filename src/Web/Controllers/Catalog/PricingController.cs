using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Queries.AdminGetPrices;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;

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
}
