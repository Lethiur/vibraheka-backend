using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Subscriptions.Commands;
using VibraHeka.Application.Subscriptions.Commands.AddSubscription;
using VibraHeka.Application.Subscriptions.Commands.CancelSubscription;
using VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Mappers;
using VibraHeka.Web.Subscriptions;

namespace VibraHeka.Web.Controllers;

public class SubscriptionController(
    IMediator mediator,
    SubscriptionMapper mapper,
    CreateSubscriptionMapper checkoutMapper,
    ILogger<SubscriptionController> logger) : ISubscriptionsController
{
    public override async Task<ActionResult<SubscriptionCreationDetailsDTO>> Subscribe()
    {
        logger.LogInformation("Subscription created successfully");
        AddSubscriptionCommand command = new();
        Result<SubscriptionCheckoutSessionEntity> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            logger.LogError("Subscription creation failed: {Error}", result.Error);
            BadRequestResponse badRequestResponse = new() { ErrorCode = result.Error };
            return new BadRequestObjectResult(badRequestResponse);
        }

        return new OkObjectResult(checkoutMapper.toDTO(result.Value));
    }

    public async override Task<ActionResult<SubscriptionEntityDTO>> GetSubscriptionStatus()
    {
        GetSubscriptionDetailsQuery query = new();
        Result<SubscriptionEntity> result = await mediator.Send(query);

        if (result.IsFailure)
        {
            logger.LogError("Failed to retrieve subscription details: {Error}", result.Error);
            BadRequestResponse badRequestResponse = new() { ErrorCode = result.Error };
            return new BadRequestObjectResult(badRequestResponse);
        }

        return new OkObjectResult(mapper.ToDetailsDTO(result.Value));
    }

    public override async Task<IActionResult> CancelSubscription()
    {
        CancelSubscriptionCommand command = new();

        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            logger.LogError("Subscription cancellation failed: {Error}", result.Error);
            BadRequestResponse badRequestResponse = new() { ErrorCode = result.Error };
            return new BadRequestObjectResult(badRequestResponse);
        }

        return new NoContentResult();
    }

    public override async Task<IActionResult> ReactivateSubscription()
    {
        ReactivateSubscriptionCommand command = new();
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            logger.LogError("Subscription reactivation failed: {Error}", result.Error);
            BadRequestResponse badRequestResponse = new() { ErrorCode = result.Error };
            return new BadRequestObjectResult(badRequestResponse);
        }

        return new NoContentResult();
    }


    public override async Task<ActionResult<SubscriptionPortalDTO>> GetSubscriptionPortal()
    {
        GetSubscriptionPortalQuery query = new();
        Result<string> result = await mediator.Send(query);
        if (result.IsFailure)
        {
            logger.LogError("Failed to retrieve subscription details: {Error}", result.Error);
            BadRequestResponse badRequestResponse = new() { ErrorCode = result.Error };
            return new BadRequestObjectResult(badRequestResponse);
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
}
