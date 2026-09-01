using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Commerce.Commands.CreateOrder;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Models;

namespace VibraHeka.Application.FunctionalTests.Commerce.Commands.CreateOrder;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class HandleTest : GenericCreateOrderFunctionalTest
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildValidUserEntity()));

        SellableItemPortMock
            .Setup(x => x.GetSellableItemByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) =>
                Result.Success(BuildSellableItemEntityFor(id)));

        SellableItemPricePortMock
            .Setup(x => x.GetSellableItemPriceById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken _) =>
                Result.Success(BuildSellableItemPriceEntityFor(id, id)));

        PaymentsPortMock
            .Setup(x => x.StartPaymentProcessAsync(It.IsAny<CheckoutOrderModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildValidPaymentAttemptEntity()));

        OrderWritePortMock
            .Setup(x => x.CreateOrder(It.IsAny<Domain.Commerce.Entities.OrderEntity>()))
            .Returns(TransactWriteOpMock.Object);

        OrderLineWritePortMock
            .Setup(x => x.CreateOrderLine(It.IsAny<Domain.Commerce.Entities.OrderLineEntity>()))
            .Returns(TransactWriteOpMock.Object);

        PaymentAttemptWritePortMock
            .Setup(x => x.CreatePaymentAttempt(It.IsAny<PaymentAttemptEntity>()))
            .Returns(TransactWriteOpMock.Object);

        AtomicWriteStoreMock
            .Setup(x => x.CommitAsync(It.IsAny<TransactionalWriteBatch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));
    }

    [Test]
    [DisplayName("Full pipeline: should process two order lines and call CreateOrderLine twice")]
    public async Task ShouldCallCreateOrderLineTwiceForCommandWithTwoLines()
    {
        // Given: a command with two distinct order lines
        CreateOrderCommand command = BuildCommandWithTwoLines();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the handler succeeds and CreateOrderLine is called once per line
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        OrderLineWritePortMock.Verify(
            x => x.CreateOrderLine(It.Is<Domain.Commerce.Entities.OrderLineEntity>(l =>
                l.SellableItemID == FakeSellableItemIdA)),
            Times.Once,
            $"Expected CreateOrderLine called once for SellableItemID='{FakeSellableItemIdA}'");

        OrderLineWritePortMock.Verify(
            x => x.CreateOrderLine(It.Is<Domain.Commerce.Entities.OrderLineEntity>(l =>
                l.SellableItemID == FakeSellableItemIdB)),
            Times.Once,
            $"Expected CreateOrderLine called once for SellableItemID='{FakeSellableItemIdB}'");
    }

    [Test]
    [DisplayName("Full pipeline: should call GetSellableItemByIdAsync once per order line")]
    public async Task ShouldCallGetSellableItemByIdAsyncOncePerOrderLine()
    {
        // Given: a command with two order lines
        CreateOrderCommand command = BuildCommandWithTwoLines();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: GetSellableItemByIdAsync is called exactly twice (once per line)
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        SellableItemPortMock.Verify(
            x => x.GetSellableItemByIdAsync(
                It.Is<string>(id => id == FakeSellableItemIdA),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetSellableItemByIdAsync called once for id='{FakeSellableItemIdA}'");

        SellableItemPortMock.Verify(
            x => x.GetSellableItemByIdAsync(
                It.Is<string>(id => id == FakeSellableItemIdB),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected GetSellableItemByIdAsync called once for id='{FakeSellableItemIdB}'");
    }

    [Test]
    [DisplayName("Full pipeline: should return CO-007 when payment initiation fails for multi-line order")]
    public async Task ShouldReturnCo007WhenPaymentFailsForMultiLineOrder()
    {
        // Given: a two-line order but payments port fails
        PaymentsPortMock
            .Setup(x => x.StartPaymentProcessAsync(It.IsAny<CheckoutOrderModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PaymentAttemptEntity>(CommerceErrors.OrderPlacementFailed));

        CreateOrderCommand command = BuildCommandWithTwoLines();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: handler returns CO-007 and does not commit the batch
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when payment initiation fails for a multi-line order");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.OrderPlacementFailed),
            $"Expected '{CommerceErrors.OrderPlacementFailed}' but got '{result.Error}'");

        AtomicWriteStoreMock.Verify(
            x => x.CommitAsync(
                It.Is<TransactionalWriteBatch>(b => b.IdempotencyKey == FakeIdempotencyKey),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected CommitAsync never called when payment initiation fails");

        AtomicWriteStoreMock.VerifyNoOtherCalls();
        OrderWritePortMock.VerifyNoOtherCalls();
        OrderLineWritePortMock.VerifyNoOtherCalls();
        PaymentAttemptWritePortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Full pipeline: should return CO-004 when GetSellableItemPriceById fails for the second line")]
    public async Task ShouldReturnCo004WhenPricePortFailsForSecondLine()
    {
        // Given: price port succeeds for first item but fails for the second
        SellableItemPricePortMock
            .Setup(x => x.GetSellableItemPriceById(
                It.Is<string>(id => id == FakeSellableItemPriceIdA),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildSellableItemPriceEntityFor(FakeSellableItemPriceIdA, FakeSellableItemIdA)));

        SellableItemPricePortMock
            .Setup(x => x.GetSellableItemPriceById(
                It.Is<string>(id => id == FakeSellableItemPriceIdB),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Domain.Catalog.Entities.SellableItemPriceEntity>(CommerceErrors.FailedToOperateWithOrderLines));

        CreateOrderCommand command = BuildCommandWithTwoLines();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the handler returns failure with CO-004
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when price port fails for the second line");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.FailedToOperateWithOrderLines),
            $"Expected '{CommerceErrors.FailedToOperateWithOrderLines}' but got '{result.Error}'");

        PaymentsPortMock.VerifyNoOtherCalls();
        AtomicWriteStoreMock.VerifyNoOtherCalls();
        OrderWritePortMock.VerifyNoOtherCalls();
        OrderLineWritePortMock.VerifyNoOtherCalls();
        PaymentAttemptWritePortMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Full pipeline: should return CheckoutURL from payment attempt entity after successful order")]
    public async Task ShouldReturnCheckoutUrlFromPaymentAttemptEntity()
    {
        // Given: a valid single-line command and the payment port returns a known CheckoutURL
        string expectedCheckoutUrl = "https://checkout.stripe.com/pay/ft_unique_session";
        PaymentAttemptEntity paymentAttempt = new()
        {
            PaymentAttemptID = Guid.NewGuid().ToString(),
            PaymentGatewayCheckoutURL = expectedCheckoutUrl,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(23)
        };

        PaymentsPortMock
            .Setup(x => x.StartPaymentProcessAsync(It.IsAny<CheckoutOrderModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(paymentAttempt));

        CreateOrderCommand command = BuildCommandWithOneLine();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the response CheckoutURL matches the payment attempt entity URL
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.CheckoutURL, Is.EqualTo(expectedCheckoutUrl),
            $"Expected CheckoutURL='{expectedCheckoutUrl}' but got '{result.Value.CheckoutURL}'");
    }
}


