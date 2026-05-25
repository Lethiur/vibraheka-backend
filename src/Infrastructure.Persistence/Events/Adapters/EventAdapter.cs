using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Events.Repositories;
using MediatR;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Domain.Events.Models;
using VibraHeka.Domain.Events.Ports.Out;

namespace Infrastructure.Persistence.Events.Adapters;

public class EventAdapter(EventRepository repository) : IEventRepositoryPort
{
    public Task<Result<EventEntity>> GetEventByIdAsync(string eventId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Result<EventEntity>> SaveEventAsync(EventEntity eventEntity, CancellationToken token)
    {
        return repository.SaveEventAsync(eventEntity, token);
    }

    public Task<Result<List<EventEntity>>> GetEventsAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken token)
    {
        return repository.GetEventsFromDateAsync(startDate, endDate, token);
    }

    public Task<Result<Unit>> DeleteEventAsync(string eventId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Result<UserEventRegistration>> RegisterAttendeeAsync(RegisterAttendeeModel model, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> UnRegisterAttendeeAsync(UnRegisterAttendeeModel model, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Unit>> UpdateEventStatusAsync(UpdateEventStatusModel model, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
