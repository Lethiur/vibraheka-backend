using CSharpFunctionalExtensions;
using VibraHeka.Domain.Events.Entities;

namespace VibraHeka.Application.Events.Queries.GetEvents;

public record GetEventsQuery(DateTimeOffset StartDate, DateTimeOffset EndDate) : IRequest<Result<List<EventEntity>>>
{

}
