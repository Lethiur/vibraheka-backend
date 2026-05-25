using CSharpFunctionalExtensions;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Domain.Events.Ports.Out;

namespace VibraHeka.Application.Events.Queries.GetEvents;

public class GetEventsQueryHandler(IEventRepositoryPort repositoryPort)
    : IRequestHandler<GetEventsQuery, Result<List<EventEntity>>>
{

    public async Task<Result<List<EventEntity>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        Result<List<EventEntity>> events =
            await repositoryPort.GetEventsAsync(request.StartDate, request.EndDate, cancellationToken);
        return events;
    }
}
