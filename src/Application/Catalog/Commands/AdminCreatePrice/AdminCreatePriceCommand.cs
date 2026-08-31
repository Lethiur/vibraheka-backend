using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Application.Catalog.Commands.AdminCreatePrice;

public record AdminCreatePriceCommand(string SellableItemID, float Price, CurrencyIsoCode Currency, bool SetToActive, BillingInterval? Interval)
    : IRequireAdmin, IRequest<Result<string>>;
