using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.CreateProduct;

public record CreateProductCommand(string Name, string Description, decimal Price, CurrencyIsoCode CurrencyCode) : IRequest<Result<string>>, IRequireAdmin
{

}
