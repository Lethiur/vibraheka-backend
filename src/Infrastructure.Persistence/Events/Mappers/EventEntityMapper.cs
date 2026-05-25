using Infrastructure.Persistence.Events.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Events.Entities;

namespace Infrastructure.Persistence.Events.Mappers;

[Mapper]
public partial class EventEntityMapper
{
    public partial EventDBModel FromDomain(EventEntity entity);

    public partial EventEntity ToDomain(EventDBModel model);
}
