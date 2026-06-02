using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.Catalog.Commands.AdminActivatePrice;

public record AdminActivatePriceCommand(string SellableItemPriceID, string SellableItemID) : IRequest<Result<Unit>>, IRequireAdmin;
