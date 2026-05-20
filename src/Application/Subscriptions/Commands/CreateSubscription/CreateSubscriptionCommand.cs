using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Subscriptions.Commands.CreateSubscription;

public record CreateSubscriptionCommand(string subscriptionPlanID) : IRequest<Result<Guid>>;
