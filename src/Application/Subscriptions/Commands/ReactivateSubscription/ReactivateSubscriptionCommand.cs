using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Subscriptions.Commands.ReactivateSubscription;

public record ReactivateSubscriptionCommand : IRequest<Result<Unit>>;
