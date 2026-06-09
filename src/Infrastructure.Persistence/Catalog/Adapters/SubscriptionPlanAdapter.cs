using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using MediatR;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class SubscriptionPlanAdapter(ISubscriptionPlanRepository repository) : ISubscriptionPlanPort
{
    public Task<Result<Unit>> DeActivateSubscriptionPlanAsync(string subscriptionPlanId, CancellationToken cancellationToken)
    {
        return repository.DeActivateSubscriptionPlanAsync(subscriptionPlanId, cancellationToken);
    }

    public Task<Result<Unit>> ActivateSubscriptionPlanAsync(string subscriptionPlanId, CancellationToken cancellationToken)
    {
        return repository.ActivateSubscriptionPlanAsync(subscriptionPlanId, cancellationToken);
    }

    public Task<Result<SubscriptionPlanEntity>> SaveSubscriptionPlanAsync(SubscriptionPlanEntity subscriptionPlanEntity, CancellationToken cancellationToken)
    {
        return repository.SaveSubscriptionPlanAsync(subscriptionPlanEntity, cancellationToken);
    }

    public Task<Result<IEnumerable<SubscriptionPlanEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return repository.GetAllAsync(cancellationToken);
    }
}
