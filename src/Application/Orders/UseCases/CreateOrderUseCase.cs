using CSharpFunctionalExtensions;
using VibraHeka.Application.Orders.Mappers;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Orders.Constants;
using VibraHeka.Domain.Orders.Entities;
using VibraHeka.Domain.Orders.Enums;
using VibraHeka.Domain.Orders.Models;
using VibraHeka.Domain.Orders.Ports.In;
using VibraHeka.Domain.Orders.Ports.Out;
using VibraHeka.Domain.Orders.Services;
using VibraHeka.Domain.Products.Entities;
using VibraHeka.Domain.Products.Ports.Out;

namespace VibraHeka.Application.Orders.UseCases;

public class CreateOrderUseCase(
    IOrderPort OrderPort,
    IProductPort ProductPort,
    CustomerService CustomerService,
    IPaymentsPort PaymentRepository
) : ICreateOrderPort
{
    public async Task<Result<OrderEntity>> ExecuteOrderAsync(ExecuteOrderModel model, CancellationToken token)
    {
        (bool _, bool isFailure, ProductEntity product, string error) =
            await ProductPort.GetProductByIdAsync(model.ProductID, token);

        if (isFailure)
        {
            return Result.Failure<OrderEntity>(error);
        }

        (bool _, bool userFailure, UserEntity user, string s) =
            await CustomerService.GetCustomerByUserIDAsync(model.UserID, token);

        if (userFailure)
        {
            return Result.Failure<OrderEntity>(error);
        }

        OrderEntityMapper mapper = new();
        OrderEntity orderToCreate =
            mapper.FromModel(model, DateTimeOffset.UtcNow, Guid.NewGuid().ToString());

        await OrderPort.CreateOrderAsync(orderToCreate, token);

        CheckoutProductModel checkoutModel = new()
        {
            ProductRef = product.ExternalProductID,
            CustomerID = user.CustomerID,
            FailureCallbackUrl = OrderConstants.FailureCallbackUrl,
            SuccessCallbackUrl = OrderConstants.SuccessCallbackUrl,
            OrderID = orderToCreate.OrderID,
            Quantity = model.Quantity
        };

        (bool _, bool isCheckoutError, CheckoutSessionCompletedModel checkoutSessionCompletedModel, string checkoutError) =
            await PaymentRepository.CreateCheckoutSessionAsync(checkoutModel, token);

        if(isCheckoutError)
        {
            return Result.Failure<OrderEntity>(checkoutError);
        } 
        

        return await OrderPort.UpdatePaymentInfoAsync(orderToCreate, checkoutSessionCompletedModel, token);
    }
}
