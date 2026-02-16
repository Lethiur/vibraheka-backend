using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.Subscriptions.Commands;

public class AddSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    IPaymentService paymentService,
    IUserService userService,
    ISubscriptionService subscriptionService) : IRequestHandler<AddSubscriptionCommand, Result<string>>
{
    public Task<Result<string>> Handle(AddSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return userService.GetUserByID(currentUserService.UserId!, cancellationToken)
            .BindTry(userEntity => subscriptionService.CreateSubscription(userEntity, cancellationToken))
            .BindTry(s =>
                paymentService.RegisterSubscriptionAsync(currentUserService.UserId!, s, cancellationToken));
    }
}
