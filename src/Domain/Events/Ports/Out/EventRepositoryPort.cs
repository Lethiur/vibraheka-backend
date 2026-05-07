using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Domain.Events.Models;

namespace VibraHeka.Domain.Events.Ports.Out;

public interface IEventRepositoryPort
{
    Task<Result<EventEntity>> GetEventByIdAsync(String eventId, CancellationToken token);
    Task<Result<EventEntity>> SaveEventAsync(EventEntity eventEntity, CancellationToken token);
    Task<Result<Unit>> DeleteEventAsync(String eventId, CancellationToken token);
    Task<Result<UserEventRegistration>> RegisterAttendeeAsync(RegisterAttendeeModel model, CancellationToken token);
    Task<Result<Unit>> UnRegisterAttendeeAsync(UnRegisterAttendeeModel model, CancellationToken token);
}
