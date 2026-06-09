using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;

namespace VibraHeka.Application.Catalog.Queries.AdminGetAllSubscriptionPlans;

public class GetAllSubscriptionPlansQueryHandler(ISubscriptionPlanPort subscriptionPlanPort) : IRequestHandler<GetAllSubscriptionPlansQuery, Result<IEnumerable<SubscriptionPlanEntity>>>

{
    public Task<Result<IEnumerable<SubscriptionPlanEntity>>> Handle(GetAllSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        return subscriptionPlanPort.GetAllAsync(cancellationToken);
    }
}
