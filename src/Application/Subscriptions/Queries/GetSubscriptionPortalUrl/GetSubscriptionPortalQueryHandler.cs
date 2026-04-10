using CSharpFunctionalExtensions;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;

public class GetSubscriptionPortalQueryHandler(ICurrentUserService currentUserService, UserProfilePort userProfilePort, PaymentsPort service) : IRequestHandler<GetSubscriptionPortalQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetSubscriptionPortalQuery request, CancellationToken cancellationToken)
    {
        return userProfilePort.GetProfileByUserId(currentUserService.UserId!, cancellationToken)
            .BindTry(profile => service.GetSubscriptionPanelUrlAsync(profile.CustomerID, cancellationToken));
    }
}
