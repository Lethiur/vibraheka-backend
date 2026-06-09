using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Queries.AdminGetAllSubscriptionPlans;

public record GetAllSubscriptionPlansQuery : IRequest<Result<IEnumerable<SubscriptionPlanEntity>>>, IRequireAdmin;
