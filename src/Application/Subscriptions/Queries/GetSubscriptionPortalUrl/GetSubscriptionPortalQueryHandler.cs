using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Payments;

namespace VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;

public class GetSubscriptionPortalQueryHandler(ICurrentUserService currentUserService, IPaymentService service) : IRequestHandler<GetSubscriptionPortalQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetSubscriptionPortalQuery request, CancellationToken cancellationToken)
    {
        return service.GetSubscriptionDetailsUrlAsync(currentUserService.UserId!, cancellationToken);
    }
}
