using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Subscriptions.Commands.AddSubscription;

public class AddSubscriptionCommand() : IRequest<Result<SubscriptionCheckoutSessionEntity>>;
