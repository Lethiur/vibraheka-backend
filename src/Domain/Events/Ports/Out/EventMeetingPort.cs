using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Events.Models;

namespace VibraHeka.Domain.Events.Ports.Out;

public interface IEventMeetingPort
{
    Task<Result<CreateEventResult>> ScheduleMeetingAsync(CreateEventModel model, CancellationToken cancellationToken);

    Task<Result<Unit>> DeleteMetingAsync(long meetingId, CancellationToken cancellationToken);

    Task<Result<RegisterAttendeeResult>> RegisterAttendeeAsync(RegisterAttendeeModel model, CancellationToken cancellationToken);

    Task<Result<Unit>> UnRegisterAttendeeAsync(UnRegisterAttendeeModel model, CancellationToken cancellationToken);
}
