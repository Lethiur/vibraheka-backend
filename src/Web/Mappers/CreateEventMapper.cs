using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Events.Commands.AdminCreateEvent;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Mappers;

[Mapper]
public partial class CreateEventMapper
{
    public partial CreateEventCommand ToCommand(CreateEventRequest request);
}
