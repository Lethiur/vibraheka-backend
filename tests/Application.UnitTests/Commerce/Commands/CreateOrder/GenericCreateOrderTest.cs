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
using VibraHeka.Domain.Orders.Services;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Ports.Out;

namespace VibraHeka.Application.UnitTests.Commerce.Commands.CreateOrder;

public abstract class GenericCreateOrderTest
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

    protected const string FakeUserId = "user-commerce-test-001";
    protected const string FakeCustomerId = "cus_test_commerce_001";
    protected const string FakeSellableItemId = "item-001";
    protected const string FakeSellableItemPriceId = "price-001";
    protected const string FakeIdempotencyKey = "idem-key-commerce-001";

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

    protected static CreateOrderCommand BuildValidCommand(string? idempotencyKey = null) =>
        new(new CreateOrderDTO
        {
            IdempotencyKey = idempotencyKey ?? FakeIdempotencyKey,
            OrderLines =
            [
                new CreateOrderLineDTO
                {
                    SellableItemID = FakeSellableItemId,
                    SellableItemPriceID = FakeSellableItemPriceId,
                    Quantity = 1
                }
            ]
        });

    protected static UserEntity BuildValidUserEntity() =>
        new(FakeUserId, "test@vibraheka.com", "Test User", FakeCustomerId);

    protected static SellableItemEntity BuildValidSellableItemEntity() =>
        new()
        {
            SellableItemID = FakeSellableItemId,
            Name = "Test Sellable Item",
            IsActive = true
        };

    protected static SellableItemPriceEntity BuildValidSellableItemPriceEntity() =>
        new()
        {
            SellableItemPriceID = FakeSellableItemPriceId,
            SellableItemID = FakeSellableItemId,
            ExternalPriceID = "price_stripe_001",
            ExternalProductID = "prod_stripe_001"
        };

    protected static PaymentAttemptEntity BuildValidPaymentAttemptEntity() =>
        new()
        {
            PaymentAttemptID = Guid.NewGuid().ToString(),
            PaymentGatewayCheckoutURL = "https://checkout.stripe.com/pay/test_session",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(23)
        };
}


