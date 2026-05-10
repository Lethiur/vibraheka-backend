using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Enums;
using VibraHeka.Domain.Orders.Models;
using VibraHeka.Domain.Orders.Ports.In;
using VibraHeka.Domain.Orders.Ports.Out;
using VibraHeka.Domain.Products.Entities;
using VibraHeka.Domain.Products.Ports.Out;

namespace VibraHeka.Application.Orders.UseCases;

public class CreateOrderUseCase(
    IOrderPort OrderPort,
    IUserRepository UserRepository,
    IProductPort ProductPort,
    IPaymentsPort PaymentRepository
) : ICreateOrderPort
{
    public async Task<Result<OrderEntity>> ExecuteOrderAsync(ExecuteOrderModel model, CancellationToken token)
    {
        (bool _, bool isFailure, ProductEntity? product, string? error) =
            await ProductPort.GetProductByIdAsync(model.ProductID, token);

        if (isFailure)
        {
            return Result.Failure<OrderEntity>(error);
        }

        (bool _, bool userFailure, UserEntity? user, string? s) = await UserRepository.GetByIdAsync(model.UserID, token);

        if (userFailure)
        {
            return Result.Failure<OrderEntity>(error);
        }
        
        OrderEntity orderToCreate = new()
        {
            ProductID = model.ProductID,
            CustomerID = user.CustomerID,
            CreatedBy = model.UserID,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = model.UserID,
            UserID = model.UserID,
            OrderType = model.OrderType,
            OrderID = Guid.NewGuid().ToString()
        };

        await OrderPort.CreateOrderAsync(orderToCreate, token);
        
        
        CheckoutProductModel checkoutModel = new()
        {
            OrderType = model.OrderType,
            ProductRef =product.ExternalProductID,
            CustomerID = user.CustomerID
        }
        
    }
}
