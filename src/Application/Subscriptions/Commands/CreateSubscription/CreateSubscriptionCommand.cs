using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Services;

namespace VibraHeka.Application.Subscriptions.Commands.CreateSubscription;

public record CreateSubscriptionCommand(string subscriptionPlanID) : IRequest<Result<Guid>>;
