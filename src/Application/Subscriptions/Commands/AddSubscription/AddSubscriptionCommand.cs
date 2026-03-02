using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Subscriptions.Commands;

public class AddSubscriptionCommand() : IRequest<Result<SubscriptionCheckoutSessionEntity>>;
