using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Subscriptions.Commands.AddSubscription;
using VibraHeka.Application.Subscriptions.Commands.CancelSubscription;
using VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Subscriptions;

namespace VibraHeka.Web.Controllers.Subscriptions;

public class SubscriptionController(
    IMediator mediator,
    SubscriptionMapper mapper,
    SubscriptionMapper subscriptionMapper,
    ILogger<SubscriptionController> logger) : ISubscriptionsController
{
    /// <summary>
    /// Creates a new subscription for the current user.
    /// </summary>
    /// <returns>An <see cref="ActionResult{SubscriptionResponse}"/> containing the subscription creation details if successful, or an error response if the operation fails.</returns>
    public override async Task<ActionResult<SubscriptionResponse>> Subscribe()
    {
        logger.LogInformation("Subscription created successfully");
        AddSubscriptionCommand command = new();
        Result<SubscriptionCheckoutSessionEntity> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            logger.LogError("Subscription creation failed: {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(subscriptionMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Retrieves the subscription status for the current user.
    /// </summary>
    /// <returns>An <see cref="ActionResult{SubscriptionDetailsResponse}"/> containing the subscription status if successful, or an error response if the operation fails.</returns>
    public async override Task<ActionResult<SubscriptionDetailsResponse>> GetSubscriptionStatus()
    {
        GetSubscriptionDetailsQuery query = new();
        Result<SubscriptionEntity> result = await mediator.Send(query);

        if (result.IsFailure)
        {
            logger.LogError("Failed to retrieve subscription details: {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(mapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Cancels the current user's subscription.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    public override async Task<IActionResult> CancelSubscription()
    {
        CancelSubscriptionCommand command = new();

        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            logger.LogError("Subscription cancellation failed: {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new NoContentResult();
    }

    /// <summary>
    /// Reactivates a previously canceled subscription for the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    public override async Task<IActionResult> ReactivateSubscription()
    {
        ReactivateSubscriptionCommand command = new();
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            logger.LogError("Subscription reactivation failed: {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new NoContentResult();
    }

    /// <summary>
    /// Retrieves the subscription portal URL for the current user, allowing them to manage their subscription settings.
    /// </summary>
    /// <returns>An <see cref="ActionResult{SubscriptionPortalResponse}"/> containing the subscription portal URL if successful, or an error response if the operation fails.</returns>
    public override async Task<ActionResult<SubscriptionPortalResponse>> GetSubscriptionPortal()
    {
        GetSubscriptionPortalQuery query = new();
        Result<string> result = await mediator.Send(query);
        if (result.IsFailure)
        {
            logger.LogError("Failed to retrieve subscription details: {Error}", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new SubscriptionPortalResponse { Url = result.Value });
    }
}
