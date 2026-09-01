using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Queries.AdminGetAllSubscriptionPlans;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Web.Catalog.Subscriptions.Controllers;

namespace VibraHeka.Web.Controllers.Catalog.SubscriptionPlans;

/// <summary>
/// Controller responsible for handling subscription plan-related operations in the system.
/// This includes retrieving, creating, and manipulating subscription plans within the catalog.
/// </summary>
public class SubscriptionPlanController(IMediator mediator, SubscriptionPlanMapper mapper) : ISubscriptionPlanController
{
    /// <summary>
    /// Retrieves a list of subscription plans from the catalog.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that allows processing to be cancelled. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="GetSubscriptionPlanResponse"/> if the operation succeeds,
    /// or a bad request response with error details if the operation fails.
    /// </returns>
    public override async Task<ActionResult<GetSubscriptionPlanResponse>> GetSubscriptionPlans(
        CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<SubscriptionPlanEntity>>
            result = await mediator.Send(new GetAllSubscriptionPlansQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }

        return Ok(new GetSubscriptionPlanResponse { Plans = [.. result.Value.Select(mapper.ToResponse)] });
    }


    /// <summary>
    /// Creates a new subscription plan based on the request data provided.
    /// </summary>
    /// <param name="body">
    /// The data required to create a subscription plan, encapsulated in the <see cref="CreateSubscriptionPlanRequest"/> object.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that allows the operation to be cancelled. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="CreateSubscriptionPlanResponse"/> if the operation succeeds,
    /// or a bad request response with error details if the operation fails.
    /// </returns>
    public override async Task<ActionResult<CreateSubscriptionPlanResponse>> CreateSubscriptionPlan(
        CreateSubscriptionPlanRequest body,
        CancellationToken cancellationToken = default)
    {
        Result<string> result = await mediator.Send(mapper.ToCommand(body), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }

        return Ok(new CreateSubscriptionPlanResponse { Id = Guid.Parse(result.Value) });
    }


}
