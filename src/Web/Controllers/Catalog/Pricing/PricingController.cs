using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Queries.AdminGetPrices;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Web.Catalog.Pricing.Controllers;

namespace VibraHeka.Web.Controllers.Catalog.Pricing;

/// <summary>
/// Handles HTTP requests related to pricing operations within the catalog.
/// </summary>]
public class PricingController(IMediator mediator, PricingMapper pricingMapper) : IPricingController
{
    /// <summary>
    /// Retrieves pricing details for a specified reference ID.
    /// </summary>
    /// <param name="id">The unique reference ID used to query pricing details.</param>
    /// <param name="cancellationToken">The cancellation token used to signal request cancellation.</param>
    /// <returns>A response containing a successful result with a SellableItemDto or an error code.</returns>
    public override async Task<ActionResult<GetPriceDetailsResponse>> GetPriceById(Guid id, CancellationToken cancellationToken = default)
    {
        AdminGetPrices query = new(id.ToString());

        Result<SellableItemEntity> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new GetPriceDetailsResponse() {  Product = pricingMapper.ToResponse(result.Value) });
    }

    /// <summary>
    /// Activates a price for a product based on the provided request body.
    /// </summary>
    /// <param name="body">The request body containing the details for the price to be activated.</param>
    /// <param name="cancellationToken">The cancellation token used to signal request cancellation.</param>
    /// <returns>The action result indicating the outcome of the activation operation.</returns>
    public override async Task<IActionResult> ActivatePrice(ActivatePriceRequest body, CancellationToken cancellationToken = default)
    {
        Result<Unit> commandResult = await mediator.Send(pricingMapper.ToCommand(body), cancellationToken);

        if (commandResult.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = commandResult.Error });
        }

        return new NoContentResult();
    }

    
    /// <summary>
    /// Creates a new price for a product based on the provided request body.
    /// </summary>
    /// <param name="body">The request body containing the details for the new price.</param>
    /// <param name="cancellationToken">The cancellation token used to signal request cancellation.</param>
    /// <returns>The action result containing the response with the created price ID.</returns>
    public override async Task<ActionResult<CreatePriceResponse>> CreatePriceForProduct(CreatePriceRequest body, CancellationToken cancellationToken = default)
    {
        Result<string> commandResult = await mediator.Send(pricingMapper.ToCommand(body), cancellationToken);

        if (commandResult.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = commandResult.Error });
        } 

        return new OkObjectResult(new CreatePriceResponse { PriceId = Guid.Parse(commandResult.Value) });
    }
}
