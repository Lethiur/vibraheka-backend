using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Commands.AdminAddSubscriptionPlan;

public record AdminAddSubscriptionPlanCommand(
    string Name,
    string Description,
    decimal Price,
    BillingInterval Interval,
    CurrencyIsoCode CurrencyCode
) : IRequest<Result<string>>, IRequireAdmin;
