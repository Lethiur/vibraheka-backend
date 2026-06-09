using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NanoidDotNet;
using NMoneys;
using VibraHeka.Application.Events.Models;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Ports.In;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Domain.Events.Enums;
using VibraHeka.Domain.Events.Errors;
using VibraHeka.Domain.Events.Models;
using VibraHeka.Domain.Events.Ports.Out;

namespace VibraHeka.Application.Events.Commands.AdminCreateEvent;

public class CreateEventCommandHandler(
    IEventMeetingPort meetingPort,
    IEventRepositoryPort repositoryPort,
    ICurrentUserService currentUser,
    IRegisterSellableItemPort sellableItemPort,
    ILogger<CreateEventCommandHandler> logger) : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        CreateEventModel model = new()
        {
            Name = request.EventName,
            Duration = request.Duration,
            EventPassword = await Nanoid.GenerateAsync(size: 10),
            EventTimezone = request.EventTimezone,
            StartDate = request.EventDate
        };

        (bool isSuccess, bool isFailure, CreateEventResult value, string error) =
            await meetingPort.ScheduleMeetingAsync(model, cancellationToken);

        if (isFailure)
        {
            logger.LogError("Failed to create event meeting: {Error}", error);
            return Result.Failure<EventDto>(EventErrors.FailedToCreateEventMeeting);
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
            return Result.Failure<EventDto>(EventErrors.FailedToCreateSellableItem);
        }

        return await repositoryPort.SaveEventAsync(entity, cancellationToken).Map(savedEntity => new EventDto()
        {
            EventID = savedEntity.EventID
        });

    }
}
