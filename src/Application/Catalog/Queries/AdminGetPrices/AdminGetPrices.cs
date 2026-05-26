using CSharpFunctionalExtensions;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Queries.AdminGetPrices;

public record AdminGetPrices(string RefID) : IRequireAdmin, IRequest<Result<SellableItemDto>>;
