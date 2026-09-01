using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Events.Commands.AdminCreateEvent;
using VibraHeka.Application.Events.Queries.GetEvents;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Web.Events;

namespace VibraHeka.Web.Controllers.Events;

[Mapper]
public partial class EventMapper
{
    [MapProperty(nameof(CreateEventRequest.StartDate), nameof(AdminCreateEventCommand.EventDate))]
    [MapProperty(nameof(CreateEventRequest.Name), nameof(AdminCreateEventCommand.EventName))]
    [MapProperty(nameof(CreateEventRequest.Description), nameof(AdminCreateEventCommand.EventDescription))]
    [MapProperty(nameof(CreateEventRequest.Timezone), nameof(AdminCreateEventCommand.EventTimezone))]
    public partial AdminCreateEventCommand ToCommand(CreateEventRequest request);
    
    [MapProperty(nameof(GetEventsRequest.From), nameof(GetEventsQuery.StartDate))]
    [MapProperty(nameof(GetEventsRequest.To), nameof(GetEventsQuery.EndDate))]
    public partial GetEventsQuery ToQuery(GetEventsRequest request);
    
    [MapProperty(nameof(EventEntity.ID), nameof(EventDTO.Id))]
    [MapProperty(nameof(EventEntity.EventPassword), nameof(EventDTO.Password))]
    [MapProperty(nameof(EventEntity.EventTimezone), nameof(EventDTO.Timezone))]
    [MapProperty(nameof(EventEntity.EventLink), nameof(EventDTO.Link))]
    [MapProperty(nameof(EventEntity.Created), nameof(EventDTO.CreatedAt))]
    [MapProperty(nameof(EventEntity.LastModified), nameof(EventDTO.LastUpdatedAt))]
    [MapProperty(nameof(EventEntity.EventDateUtc), nameof(EventDTO.StartDate))]
    [MapperIgnoreSource(nameof(EventEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(EventEntity.LastModifiedBy))]
    [MapperIgnoreSource(nameof(EventEntity.EventID))]
    public partial EventDTO ToResponse(EventEntity entity);
    
    [MapProperty(nameof(id), nameof(CreateEventResponse.Id))]
    public partial CreateEventResponse ToResponse(string id);
}
