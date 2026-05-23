using CSharpFunctionalExtensions;
using VibraHeka.Application.Events.Models;

namespace VibraHeka.Application.Events.Commands.AdminCreateEvent;

public class CreateEventCommandHandler() : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    public Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
