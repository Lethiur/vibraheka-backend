using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Queries.AdminGetPrices;

public record AdminGetPrices(string RefID) : IRequireAdmin, IRequest<Result<SellableItemEntity>>;
