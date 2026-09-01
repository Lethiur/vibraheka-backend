using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Events.Commands.AdminCreateEvent;

public record AdminCreateEventCommand(
    string EventName,
    string EventDescription,
    DateTimeOffset EventDate,
    int Duration,
    string EventTimezone,
    decimal Price,
    CurrencyIsoCode CurrencyCode) : IRequireAdmin, IRequest<Result<string>>
{
}
