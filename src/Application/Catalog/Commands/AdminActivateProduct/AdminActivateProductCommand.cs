using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Commands.AdminActivateProduct;

public record AdminActivateProductCommand(ProductType Type, string ProductID) : IRequest<Result<Unit>>, IRequireAdmin;
