using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Subscriptions.Queries.GetSubscriptionPortalUrl;

public record GetSubscriptionPortalQuery : IRequest<Result<string>>;
