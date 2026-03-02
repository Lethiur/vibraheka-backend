using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SubscriptionMapper
{
    public partial SubscriptionDetailsDTO ToDetailsDTO(SubscriptionEntity entity);
}
