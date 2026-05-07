using CSharpFunctionalExtensions;
using VibraHeka.Application.Events.Models;
using VibraHeka.Domain.Common;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Products.Ports.Out;

namespace VibraHeka.Application.Events.Commands.AdminCreateEvent;

public class CreateEventCommandHandler(
    IEventMeetingPort EventMeetingPort,
    IEventRepositoryPort EventRepository,
    ICurrentUserService CurrentUserService
    ) : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    public Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Building Event
       // EventEntity.
        
        
        throw new NotImplementedException();
    }
}
