using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.AdminCreatePrice;

public record AdminCreatePriceCommand(string SellableItemID, decimal Price, CurrencyIsoCode Currency, bool SetToActive)
    : IRequireAdmin, IRequest<Result<string>>;
