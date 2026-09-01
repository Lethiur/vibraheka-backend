using CSharpFunctionalExtensions;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Application.Commerce.Ports.Out;
using VibraHeka.Application.Payments.Ports.Out;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Domain.Commerce.Factories;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Models;
using VibraHeka.Domain.Payments.Ports.Out;
using VibraHeka.Domain.Payments.Services;

namespace VibraHeka.Application.Commerce.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    CustomerService customerService,
    IAtomicWriteStore transactionStore,
    ISellableItemPricePort sellableItemPricePort,
    ISellableItemPort sellableItemPort,
    IOrderWritePort orderWritePort,
    IOrderLineWritePort orderLineWritePort,
    IPaymentAttemptWritePort paymentAttemptWritePort,
    IPaymentsPort paymentsPort,
    ICurrentUserService currentUserService) : IRequestHandler<CreateOrderCommand, Result<OrderCheckoutModel>>
{
    public async Task<Result<OrderCheckoutModel>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        if (request.dto.OrderLines.Count == 0)
        {
            return Result.Failure<OrderCheckoutModel>(CommerceErrors.InvalidOrderLines);
        }

        (bool _, bool userRetrievalFailure, UserEntity userEntity) =
            await customerService.GetCustomerByUserIDAsync(currentUserService.UserId!, cancellationToken);

        if (userRetrievalFailure)
        {
            return Result.Failure<OrderCheckoutModel>(CommerceErrors.FailedToOperateWithOrderLines);
        }

        OrderEntity orderEntity = OrderFactory.ForUser(currentUserService.UserId!);

        foreach (CreateOrderLineDTO orderLine in request.dto.OrderLines)
        {
            (bool _, bool isFailure, OrderLineEntity orderLineEntity) = await
                GetOrderLineAsync(orderLine, cancellationToken);

            if (isFailure)
            {
                return Result.Failure<OrderCheckoutModel>(CommerceErrors.FailedToOperateWithOrderLines);
            }
            orderLineEntity.Quantity = orderLine.Quantity;
            orderEntity.AddLine(orderLineEntity);
        }

        CheckoutOrderModel checkoutOrderModel = new CheckoutOrderModel()
        {
            CustomerID = userEntity.CustomerID,
            Order = orderEntity,
            SuccessCallbackUrl = "https://www.vibraheka.com/profile/me",
            CancelCallbackUrl = "https://vibraheka.com/profile/me",
            PaymentMethodsAccepted = ["card", "paypal", "klarna"]
        };

        (bool _, bool paymentFailed, PaymentAttemptEntity? value) =
            await paymentsPort.StartPaymentProcessAsync(checkoutOrderModel, cancellationToken);

        if (paymentFailed)
        {
            return Result.Failure<OrderCheckoutModel>(CommerceErrors.OrderPlacementFailed);
        }

        value.LinkOrder(orderEntity);

        TransactionalWriteBatch batch = new TransactionalWriteBatch(request.dto.IdempotencyKey);
        batch.Add(orderWritePort.CreateOrder(orderEntity));
        batch.AddRange(orderEntity.Lines.Select(orderLineWritePort.CreateOrderLine));
        batch.Add(paymentAttemptWritePort.CreatePaymentAttempt(value));

        return await transactionStore.CommitAsync(batch, cancellationToken).Map(_ => new OrderCheckoutModel()
        {
            CheckoutURL = value.PaymentGatewayCheckoutURL,
            ExpiresAtUTC = value.ExpiresAt
        });
    }

    /// <summary>
    /// Asynchronously retrieves and creates an <see cref="OrderLineEntity"/> based on the provided order line data,
    /// user identifier, and cancellation token.
    /// </summary>
    /// <param name="orderLine">
    ///     The data transfer object containing order line details, such as the sellable item ID.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="Result{OrderLineEntity}"/> containing the created <see cref="OrderLineEntity"/> if the operation is successful,
    /// or an error if the process fails.
    /// </returns>
    private Task<Result<OrderLineEntity>> GetOrderLineAsync(CreateOrderLineDTO orderLine,
        CancellationToken cancellationToken)
    {
        return sellableItemPort.GetSellableItemByIdAsync(orderLine.SellableItemID, cancellationToken)
            .BindTry(sellableItemFromDynamo =>
            {
                return sellableItemPricePort.GetSellableItemPriceById(orderLine.SellableItemPriceID, cancellationToken)
                    .Map(sellablePriceFromDynamo => OrderLineEntityFactory.FromSellableInformation(
                        sellableItemFromDynamo, sellablePriceFromDynamo,
                        currentUserService.UserId!));
            });
    }
}
