using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.In;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.AdminAddSubscriptionPlan;

public class AdminAddSubscriptionPlanCommandHandler(
    ISubscriptionPlanPort subscriptionPlanPort,
    IRegisterSellableItemPort SellableItemPort,
    ICurrentUserService CurrentUserService,
    ILogger<AdminAddSubscriptionPlanCommandHandler> Logger)
    : IRequestHandler<AdminAddSubscriptionPlanCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AdminAddSubscriptionPlanCommand request,
        CancellationToken cancellationToken)
    {
        string recordingId = Guid.NewGuid().ToString();

        Logger.LogInformation("Creating subscription plan entry {RecordingId} with name {Name}", recordingId,
            request.Name);

        SubscriptionPlanEntity entity = new()
        {
            SubscriptionPlanID = recordingId,
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = CurrentUserService.UserId,
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = CurrentUserService.UserId
        };

        Result<SubscriptionPlanEntity> saveResult = await subscriptionPlanPort.SaveSubscriptionPlanAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
        {
            Logger.LogWarning("Failed to persist recording {RecordingId}: {Error}", recordingId, saveResult.Error);
            return Result.Failure<string>(saveResult.Error);
        }

        Result<Unit> subscriptionPlanRegistrationAsync =
            await SellableItemPort.RegisterSellableItemAsync(entity, new Money(request.Price, request.CurrencyCode),
                PriceKind.OneTime, request.Interval, cancellationToken);

        if (subscriptionPlanRegistrationAsync.IsFailure)
        {
            Logger.LogWarning("Failed to register sellable item for recording {RecordingId}: {Error}", recordingId,
                subscriptionPlanRegistrationAsync.Error);
            return Result.Failure<string>(subscriptionPlanRegistrationAsync.Error);
        }

        Logger.LogInformation(
            "Subscription plan {RecordingId} persisted",
            recordingId);
        return Result.Success(entity.SubscriptionPlanID);
    }
}
