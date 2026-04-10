using CSharpFunctionalExtensions;
using VibraHeka.Domain.Subscriptions.Entities;

namespace VibraHeka.Application.Subscriptions.Commands;

public class AddSubscriptionCommand() : IRequest<Result<SubscriptionCheckoutSessionEntity>>;
