using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Commerce.Commands.CreateOrder;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Application.Commerce.Ports.Out;
using VibraHeka.Application.Payments.Ports.Out;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Ports.Out;
using VibraHeka.Domain.Payments.Services;

namespace VibraHeka.Application.FunctionalTests.Commerce.Commands.CreateOrder;

public abstract class GenericCreateOrderFunctionalTest
{
    protected Mock<IUserRepository> UserRepositoryMock = default!;
    protected Mock<IPaymentsPort> CustomerServicePaymentsPortMock = default!;
    protected CustomerService CustomerServiceInstance = default!;
    protected Mock<IAtomicWriteStore> AtomicWriteStoreMock = default!;
    protected Mock<ISellableItemPricePort> SellableItemPricePortMock = default!;
    protected Mock<ISellableItemPort> SellableItemPortMock = default!;
    protected Mock<IOrderWritePort> OrderWritePortMock = default!;
    protected Mock<IOrderLineWritePort> OrderLineWritePortMock = default!;
    protected Mock<IPaymentAttemptWritePort> PaymentAttemptWritePortMock = default!;
    protected Mock<IPaymentsPort> PaymentsPortMock = default!;
    protected Mock<ICurrentUserService> CurrentUserServiceMock = default!;
    protected Mock<ITransactionalWriteOperation> TransactWriteOpMock = default!;
    protected CreateOrderCommandHandler Handler = default!;

    protected const string FakeUserId = "user-ft-commerce-001";
    protected const string FakeCustomerId = "cus_ft_commerce_001";
    protected const string FakeSellableItemIdA = "item-ft-001";
    protected const string FakeSellableItemIdB = "item-ft-002";
    protected const string FakeSellableItemPriceIdA = "price-ft-001";
    protected const string FakeSellableItemPriceIdB = "price-ft-002";
    protected const string FakeIdempotencyKey = "idem-key-ft-commerce-001";

    [SetUp]
    public virtual void SetUp()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        CustomerServicePaymentsPortMock = new Mock<IPaymentsPort>();
        AtomicWriteStoreMock = new Mock<IAtomicWriteStore>();
        SellableItemPricePortMock = new Mock<ISellableItemPricePort>();
        SellableItemPortMock = new Mock<ISellableItemPort>();
        OrderWritePortMock = new Mock<IOrderWritePort>();
        OrderLineWritePortMock = new Mock<IOrderLineWritePort>();
        PaymentAttemptWritePortMock = new Mock<IPaymentAttemptWritePort>();
        PaymentsPortMock = new Mock<IPaymentsPort>();
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        TransactWriteOpMock = new Mock<ITransactionalWriteOperation>();

        CustomerServiceInstance = new CustomerService(
            UserRepositoryMock.Object,
            CustomerServicePaymentsPortMock.Object,
            new Mock<ILogger<CustomerService>>().Object);

        CurrentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(FakeUserId);

        Handler = new CreateOrderCommandHandler(
            CustomerServiceInstance,
            AtomicWriteStoreMock.Object,
            SellableItemPricePortMock.Object,
            SellableItemPortMock.Object,
            OrderWritePortMock.Object,
            OrderLineWritePortMock.Object,
            PaymentAttemptWritePortMock.Object,
            PaymentsPortMock.Object,
            CurrentUserServiceMock.Object);
    }

    protected static CreateOrderCommand BuildCommandWithTwoLines() =>
        new(new CreateOrderDTO
        {
            IdempotencyKey = FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO
                {
                    SellableItemID = FakeSellableItemIdA,
                    SellableItemPriceID = FakeSellableItemPriceIdA,
                    Quantity = 1
                },
                new CreateOrderLineDTO
                {
                    SellableItemID = FakeSellableItemIdB,
                    SellableItemPriceID = FakeSellableItemPriceIdB,
                    Quantity = 2
                }
            ]
        });

    protected static CreateOrderCommand BuildCommandWithOneLine(string? idempotencyKey = null) =>
        new(new CreateOrderDTO
        {
            IdempotencyKey = idempotencyKey ?? FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO
                {
                    SellableItemID = FakeSellableItemIdA,
                    SellableItemPriceID = FakeSellableItemPriceIdA,
                    Quantity = 1
                }
            ]
        });

    protected static UserEntity BuildValidUserEntity() =>
        new(FakeUserId, "ft-test@vibraheka.com", "FT Test User", FakeCustomerId);

    protected static SellableItemEntity BuildSellableItemEntityFor(string sellableItemId) =>
        new()
        {
            SellableItemID = sellableItemId,
            Name = $"Test Sellable Item {sellableItemId}",
            IsActive = true
        };

    protected static SellableItemPriceEntity BuildSellableItemPriceEntityFor(string priceId, string sellableItemId) =>
        new()
        {
            SellableItemPriceID = priceId,
            SellableItemID = sellableItemId,
            ExternalPriceID = $"price_stripe_{priceId}",
            ExternalProductID = $"prod_stripe_{sellableItemId}"
        };

    protected static PaymentAttemptEntity BuildValidPaymentAttemptEntity() =>
        new()
        {
            PaymentAttemptID = Guid.NewGuid().ToString(),
            PaymentGatewayCheckoutURL = "https://checkout.stripe.com/pay/ft_session",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(23)
        };
}

