using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;

namespace Infrastructure.Persistence.Catalog.Repositories;

public interface ISubscriptionPlanRepository
{
    public Task<Result<Unit>> DeActivateSubscriptionPlanAsync(string subscriptionPlanId,
        CancellationToken cancellationToken);

    public Task<Result<Unit>> ActivateSubscriptionPlanAsync(string subscriptionPlanId,
        CancellationToken cancellationToken);
    
    Task<Result<IEnumerable<SubscriptionPlanEntity>>> GetAllAsync(CancellationToken cancellationToken);
    
    
    public Task<Result<SubscriptionPlanEntity>> SaveSubscriptionPlanAsync(SubscriptionPlanEntity entity, CancellationToken cancellationToken);
}
