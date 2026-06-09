using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface ISubscriptionPlanPort
{
    public Task<Result<Unit>> DeActivateSubscriptionPlanAsync(string subscriptionPlanId,
        CancellationToken cancellationToken);
    
    public Task<Result<Unit>> ActivateSubscriptionPlanAsync(string subscriptionPlanId,
        CancellationToken cancellationToken);
    
    public Task<Result<SubscriptionPlanEntity>> SaveSubscriptionPlanAsync(SubscriptionPlanEntity subscriptionPlanEntity,
        CancellationToken cancellationToken);
    
    public Task<Result<IEnumerable<SubscriptionPlanEntity>>> GetAllAsync(CancellationToken cancellationToken);
}
