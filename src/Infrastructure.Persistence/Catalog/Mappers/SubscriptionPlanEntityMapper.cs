using Infrastructure.Persistence.Catalog.Models;
using Riok.Mapperly.Abstractions;
using VibraHeka.Domain.Catalog.Entities;
namespace Infrastructure.Persistence.Catalog.Mappers;

[Mapper]
public partial class SubscriptionPlanEntityMapper
{
    public partial SubscriptionPlanDBModel FromDomain(SubscriptionPlanEntity entity);
    public partial SubscriptionPlanEntity ToDomain(SubscriptionPlanDBModel model);
}
