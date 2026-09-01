using CSharpFunctionalExtensions;
using VibraHeka.Application.Commerce.Models;

namespace VibraHeka.Application.Commerce.Commands.CreateOrder;

public record CreateOrderCommand(CreateOrderDTO dto) : IRequest<Result<OrderCheckoutModel>>;
