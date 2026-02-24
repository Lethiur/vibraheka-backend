using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Subscriptions.Queries.GetSubscriptionDetails;

public record GetSubscriptionDetailsQuery : IRequest<Result<SubscriptionEntity>>;
