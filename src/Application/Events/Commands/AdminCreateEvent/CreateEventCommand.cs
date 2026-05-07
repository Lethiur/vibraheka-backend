using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Events.Models;

namespace VibraHeka.Application.Events.Commands.AdminCreateEvent;

public record CreateEventCommand(
    string EventName,
    string EventDescription,
    DateTime EventDate,
    int Duration,
    string EventTimezone,
    string? ProductID) : IRequireAdmin, IRequest<Result<EventDto>>
{
}
