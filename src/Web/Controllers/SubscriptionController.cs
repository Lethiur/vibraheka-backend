using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Subscriptions.Commands;
using VibraHeka.Application.Subscriptions.Commands.CancelSubscription;
using VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;
using VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Mappers;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/subscriptions")]
public class SubscriptionController(
    IMediator mediator,
    SubscriptionMapper mapper,
    ILogger<SubscriptionController> Logger)
{
    [HttpPut]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> Subscribe()
    {
        Logger.LogCritical("NAMESPACE DE ESTE LOGGER: " + typeof(SubscriptionController).Namespace);
        AddSubscriptionCommand command = new();
        Result<string> result = await mediator.Send(command);
        Logger.LogInformation("Subscription created successfully!!@#!@#!@#!@#");
        if (result.IsFailure)
        {
            Logger.LogError("Subscription creation failed: {Error}", result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [HttpPatch("reactivate")]
    [Authorize]
    public async Task<IActionResult> ReactivateSubscription()
    {
        ReactivateSubscriptionCommand command = new();
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            Logger.LogError("Subscription reactivation failed: {Error}", result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }

    [HttpGet("details")]
    [Authorize]
    public async Task<IActionResult> GetSubscriptionPortal()
    {
        GetSubscriptionPortalQuery query = new();
        Result<string> result = await mediator.Send(query);
        if (result.IsFailure)
        {
            Logger.LogError("Failed to retrieve subscription details: {Error}", result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> GetSubscriptionDetails()
    {
        GetSubscriptionDetailsQuery query = new();

        Result<SubscriptionEntity> result = await mediator.Send(query);

        if (result.IsFailure)
        {
            Logger.LogError("Failed to retrieve subscription details: {Error}", result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(mapper.ToDetailsDTO(result.Value)));
    }

    [HttpPatch]
    [Authorize]
    [Produces("application/json")]
    public async Task<IActionResult> UpdateSubscription()
    {
        CancelSubscriptionCommand command = new();
        
        Result<Unit> result = await mediator.Send(command);
        
        if (result.IsFailure)
        {
            Logger.LogError("Subscription cancellation failed: {Error}", result.Error);
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        
        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }
}
