using Riok.Mapperly.Abstractions;
using VibraHeka.Application.Catalog.Commands.AdminAddSubscriptionPlan;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Web.Catalog.Subscriptions.Controllers;

namespace VibraHeka.Web.Controllers.Catalog.SubscriptionPlans;

[Mapper]
public partial class SubscriptionPlanMapper
{
    [MapProperty(nameof(SubscriptionPlanEntity.ID), nameof(SubscriptionPlanDTO.Id))]
    [MapProperty(nameof(SubscriptionPlanEntity.Created), nameof(SubscriptionPlanDTO.CreatedAt))]
    [MapProperty(nameof(SubscriptionPlanEntity.LastModified), nameof(SubscriptionPlanDTO.LastUpdatedAt))]
    [MapperIgnoreSource(nameof(SubscriptionPlanEntity.SubscriptionPlanID))]
    [MapperIgnoreSource(nameof(SubscriptionPlanEntity.CreatedBy))]
    [MapperIgnoreSource(nameof(SubscriptionPlanEntity.LastModifiedBy))]
    public partial SubscriptionPlanDTO ToResponse(SubscriptionPlanEntity entity);
    
    [MapProperty(nameof(CreateSubscriptionPlanRequest.Currency), nameof(AdminAddSubscriptionPlanCommand.CurrencyCode))]
    [MapProperty(nameof(CreateSubscriptionPlanRequest.BillingInterval), nameof(AdminAddSubscriptionPlanCommand.Interval))]
    public partial AdminAddSubscriptionPlanCommand ToCommand(CreateSubscriptionPlanRequest entity);
}
