using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NMoneys;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.In;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Domain.Events.Enums;
using VibraHeka.Domain.Events.Errors;
using VibraHeka.Domain.Events.Models;
using VibraHeka.Domain.Events.Ports.Out;

namespace VibraHeka.Application.Events.Commands.AdminCreateEvent;

public class AdminCreateEventCommandHandler(
    IEventMeetingPort meetingPort,
    IEventRepositoryPort repositoryPort,
    ICurrentUserService currentUser,
    IRegisterSellableItemPort sellableItemPort,
    ILogger<AdminCreateEventCommandHandler> logger) : IRequestHandler<AdminCreateEventCommand, Result<string>>
{
    public async Task<Result<string>> Handle(AdminCreateEventCommand request, CancellationToken cancellationToken)
    {
        CreateEventModel model = new()
        {
            Name = request.EventName,
            Duration = request.Duration,
            EventPassword = Guid.NewGuid().ToString(),
            EventTimezone = request.EventTimezone,
            StartDate = request.EventDate
        };

        (bool _, bool isFailure, CreateEventResult value, string error) =
            await meetingPort.ScheduleMeetingAsync(model, cancellationToken);

        if (isFailure)
        {
            logger.LogError("Failed to create event meeting: {Error}", error);
            return Result.Failure<string>(EventErrors.FailedToCreateEventMeeting);
        }

        EventEntity entity = new()
        {
            Duration = request.Duration,
            EventDateUtc = request.EventDate,
            Description = request.EventDescription,
            Name = request.EventName,
            EventTimezone = request.EventTimezone,
            EventPassword = model.EventPassword,
            EventID = Guid.NewGuid().ToString(),
            Created = DateTimeOffset.UtcNow,
            CreatedBy = currentUser.UserId,
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = currentUser.UserId,
            Status = EventStatus.Ready,
            EventLink = value.JoinURL
        };

        (bool _, bool sellableItemRegistrationFailure) = await sellableItemPort.RegisterSellableItemAsync(entity,
            new Money(request.Price, request.CurrencyCode), PriceKind.OneTime, null, cancellationToken);

        if (sellableItemRegistrationFailure)
        {
            return Result.Failure<string>(EventErrors.FailedToCreateSellableItem);
        }

        return await repositoryPort.SaveEventAsync(entity, cancellationToken).Map(savedEntity => savedEntity.EventID);
        
    }
}
