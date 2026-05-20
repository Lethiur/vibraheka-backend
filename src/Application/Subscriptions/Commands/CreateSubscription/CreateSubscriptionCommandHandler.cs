using CSharpFunctionalExtensions;
using NMoneys;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Models;
using VibraHeka.Domain.Catalog.Services;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Enums;
using VibraHeka.Domain.Commerce.Ports.Out;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Enums;

namespace VibraHeka.Application.Subscriptions.Commands.CreateSubscription;

public class CreateSubscriptionCommandHandler(
    ICurrentUserService currentUserService,
    SellableItemService sellableItemService,
    IOrderPort orderPort) : IRequestHandler<CreateSubscriptionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        (bool _, bool isFailure, SellableInformationModel? itemPriceEntity, string? error) =
            await sellableItemService.GetSellableItemPriceByProductReferenceAndPriceKindAsync(
                request.subscriptionPlanID,
                PriceKind.Recurring, cancellationToken);

        if (isFailure)
        {
            return Result.Failure<Guid>(error);
        }

        OrderEntity order = new()
        {
            OrderID = Guid.NewGuid().ToString(),
            DiscountTotal = Money.Zero(),
            Status = OrderStatus.Draft,
            Total = itemPriceEntity.Price.Amount,
            Subtotal = itemPriceEntity.Price.Amount,
            TaxTotal = Money.Zero(),
            UserID = currentUserService.UserId!,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = currentUserService.UserId!,
            LastModifiedBy = currentUserService.UserId!,
        };


        OrderLineEntity orderLineEntity = new()
        {
            Quantity = 1,
            Subtotal = itemPriceEntity.Price.Amount,
            Total = itemPriceEntity.Price.Amount,
            UnitPrice = itemPriceEntity.Price.Amount,
            DiscountAmount = Money.Zero(),
            TaxAmount = Money.Zero(),
            OrderID = order.OrderID,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = currentUserService.UserId!,
            LastModifiedBy = currentUserService.UserId!,
            OrderLineID = Guid.NewGuid().ToString(),
            SellablePriceID = itemPriceEntity.Price.SellableItemPriceID,
            PaymentGatewayPriceIDSnapshot = itemPriceEntity.Price.ExternalPriceID,
            PaymentGatewayProductIDSnapshot = itemPriceEntity.Price.ExternalProductID,
            Type = SellableItemType.SubscriptionPlan,
            SellableItemID = itemPriceEntity.Item.SellableItemID,
            NameSnapshot = itemPriceEntity.Item.Name
        };

        order.Lines.Add(orderLineEntity);

        Result<OrderEntity> orderAsync = await orderPort.CreateOrderAsync(order, cancellationToken);


        PaymentAttemptEntity paymentAttemptEntity = new()
        {
            Amount = order.Total,
            OrderId = order.OrderID,
            PaymentAttemptID = Guid.NewGuid().ToString(),
            Provider = PaymentsProviders.Stripe,
            Status = PaymentsStatus.Pending,
            UserId = currentUserService.UserId!,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = currentUserService.UserId!,
            LastModifiedBy = currentUserService.UserId!,
        };

        throw new NotImplementedException();
    }
}
