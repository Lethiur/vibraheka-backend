using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Commands.AdminAddSubscriptionPlan;
using VibraHeka.Application.Catalog.Queries.AdminGetAllSubscriptionPlans;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Web.Controllers.Catalog.SubscriptionPlans;

[ApiController]
[Route("api/v1/catalog/subscriptions")]
public class SubscriptionPlanController(IMediator mediator)
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllSubscriptionPlan(CancellationToken ct)
    {
        Result<IEnumerable<SubscriptionPlanEntity>>
            result = await mediator.Send(new GetAllSubscriptionPlansQuery(), ct);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanRequest request,
        CancellationToken ct)
    {
        Result<string> result =
            await mediator.Send(
                new AdminAddSubscriptionPlanCommand(request.Name, request.Description, request.Price,
                    request.BillingInterval, request.CurrencyCode), ct);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
}
