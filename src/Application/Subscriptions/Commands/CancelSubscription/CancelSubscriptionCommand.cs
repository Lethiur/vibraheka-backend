using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Subscriptions.Commands.CancelSubscription;

public record CancelSubscriptionCommand() : IRequest<Result<Unit>>;
