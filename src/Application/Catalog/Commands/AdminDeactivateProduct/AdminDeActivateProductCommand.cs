using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Commands.AdminDeactivateProduct;

public record AdminDeActivateProductCommand(ProductType Type, string ProductID) : IRequest<Result<Unit>>, IRequireAdmin;
