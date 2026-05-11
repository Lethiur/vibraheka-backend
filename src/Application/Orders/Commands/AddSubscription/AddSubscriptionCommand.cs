using CSharpFunctionalExtensions;
using VibraHeka.Domain.Orders.Models;

namespace VibraHeka.Application.Orders.Commands.AddSubscription;

public record AddSubscriptionCommand(string SubscriptionID) : IRequest<Result<CheckoutSessionCompletedModel>>;
