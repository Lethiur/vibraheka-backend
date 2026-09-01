using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Commerce.Commands.CreateOrder;
using VibraHeka.Application.Commerce.Models;
using VibraHeka.Domain.Commerce.Errors;
using VibraHeka.Domain.Common.Errors;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Models;

namespace VibraHeka.Application.UnitTests.Commerce.Commands.CreateOrder;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateOrderCommandHandlerTest : GenericCreateOrderTest
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        // Default happy-path setup shared across tests
        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildValidUserEntity()));

        SellableItemPortMock
            .Setup(x => x.GetSellableItemByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildValidSellableItemEntity()));

        SellableItemPricePortMock
            .Setup(x => x.GetSellableItemPriceById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(BuildValidSellableItemPriceEntity()));

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

    #region Happy Path

    [Test]
    [DisplayName("Should return success with CheckoutURL when all ports succeed and transaction commits")]
    public async Task ShouldReturnSuccessWithCheckoutUrlWhenAllPortsSucceed()
    {
        // Given: all mocks configured for happy path (in SetUp)
        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is success with a non-empty CheckoutURL
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.CheckoutURL, Is.Not.Null.And.Not.Empty,
            $"Expected non-empty CheckoutURL but got: '{result.Value.CheckoutURL}'");
    }

    [Test]
    [DisplayName("Should call CommitAsync with the correct IdempotencyKey from the command")]
    public async Task ShouldCallCommitAsyncWithCorrectIdempotencyKey()
    {
        // Given: a command with a specific idempotency key
        string expectedKey = "idem-test-verify-key";
        CreateOrderCommand command = BuildValidCommand(idempotencyKey: expectedKey);

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: CommitAsync is called once with a batch whose IdempotencyKey matches the command
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        AtomicWriteStoreMock.Verify(
            x => x.CommitAsync(
                It.Is<TransactionalWriteBatch>(b => b.IdempotencyKey == expectedKey),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected CommitAsync called once with IdempotencyKey='{expectedKey}'");
    }

    [Test]
    [DisplayName("Should add CreateOrder, CreateOrderLine and CreatePaymentAttempt to the batch")]
    public async Task ShouldAddAllWriteOperationsToBatch()
    {
        // Given: a valid command (mocks from SetUp)
        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: each write-port method is called once
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        OrderWritePortMock.Verify(
            x => x.CreateOrder(It.Is<Domain.Commerce.Entities.OrderEntity>(o => o.UserID == FakeUserId)),
            Times.Once,
            "Expected CreateOrder called once with an entity bearing the correct UserId");

        OrderLineWritePortMock.Verify(
            x => x.CreateOrderLine(It.Is<Domain.Commerce.Entities.OrderLineEntity>(l =>
                l.SellableItemID == FakeSellableItemId)),
            Times.Once,
            "Expected CreateOrderLine called once per order line in the command");

        PaymentAttemptWritePortMock.Verify(
            x => x.CreatePaymentAttempt(It.Is<PaymentAttemptEntity>(p =>
                p.OrderId != string.Empty && p.UserId == FakeUserId)),
            Times.Once,
            "Expected CreatePaymentAttempt called once with the linked payment attempt entity");
    }

    [Test]
    [DisplayName("Should call StartPaymentProcessAsync with CustomerID from the user entity")]
    public async Task ShouldCallStartPaymentProcessAsyncWithCustomerIdFromUserEntity()
    {
        // Given: a valid command and user entity with a known CustomerID
        CreateOrderCommand command = BuildValidCommand();
        UserEntity user = BuildValidUserEntity();

        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.Is<string>(id => id == FakeUserId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: StartPaymentProcessAsync called with the CustomerID from the user
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        PaymentsPortMock.Verify(
            x => x.StartPaymentProcessAsync(
                It.Is<CheckoutOrderModel>(m => m.CustomerID == FakeCustomerId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected StartPaymentProcessAsync called with CustomerID='{FakeCustomerId}'");
    }

    #endregion

    #region Failure — User Not Found

    [Test]
    [DisplayName("Should return failure with CO-004 when user is not found in repository")]
    public async Task ShouldReturnFailureWithCo004WhenUserNotFound()
    {
        // Given: the user repository returns failure
        UserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserEntity>(CommerceErrors.FailedToOperateWithOrderLines));

        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with CO-004
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when user is not found but got success");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.FailedToOperateWithOrderLines),
            $"Expected '{CommerceErrors.FailedToOperateWithOrderLines}' but got '{result.Error}'");

        UserRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.Is<string>(id => id == FakeUserId),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected GetByIdAsync called once");

        PaymentsPortMock.VerifyNoOtherCalls();
        AtomicWriteStoreMock.VerifyNoOtherCalls();
        SellableItemPortMock.VerifyNoOtherCalls();
        SellableItemPricePortMock.VerifyNoOtherCalls();
        OrderWritePortMock.VerifyNoOtherCalls();
        OrderLineWritePortMock.VerifyNoOtherCalls();
        PaymentAttemptWritePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Failure — GetOrderLine Fails

    [Test]
    [DisplayName("Should return failure with CO-004 when GetSellableItemByIdAsync fails for an order line")]
    public async Task ShouldReturnFailureWithCo004WhenGetSellableItemFails()
    {
        // Given: the item port returns failure
        SellableItemPortMock
            .Setup(x => x.GetSellableItemByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Domain.Catalog.Entities.SellableItemEntity>(CommerceErrors.FailedToOperateWithOrderLines));

        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with CO-004
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when item retrieval fails but got success");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.FailedToOperateWithOrderLines),
            $"Expected '{CommerceErrors.FailedToOperateWithOrderLines}' but got '{result.Error}'");

        PaymentsPortMock.VerifyNoOtherCalls();
        AtomicWriteStoreMock.VerifyNoOtherCalls();
        OrderWritePortMock.VerifyNoOtherCalls();
        OrderLineWritePortMock.VerifyNoOtherCalls();
        PaymentAttemptWritePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Failure — Payment Process Fails

    [Test]
    [DisplayName("Should return failure with CO-007 when StartPaymentProcessAsync fails")]
    public async Task ShouldReturnFailureWithCo007WhenPaymentFails()
    {
        // Given: the payments port returns failure
        PaymentsPortMock
            .Setup(x => x.StartPaymentProcessAsync(It.IsAny<CheckoutOrderModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PaymentAttemptEntity>(CommerceErrors.OrderPlacementFailed));

        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with CO-007
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when payment initiation fails but got success");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.OrderPlacementFailed),
            $"Expected '{CommerceErrors.OrderPlacementFailed}' but got '{result.Error}'");

        AtomicWriteStoreMock.Verify(
            x => x.CommitAsync(
                It.Is<TransactionalWriteBatch>(b => b.IdempotencyKey == FakeIdempotencyKey),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Never,
            "Expected CommitAsync never called when payment fails");

        AtomicWriteStoreMock.VerifyNoOtherCalls();
        OrderWritePortMock.VerifyNoOtherCalls();
        OrderLineWritePortMock.VerifyNoOtherCalls();
        PaymentAttemptWritePortMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Failure — Commit Fails

    [Test]
    [DisplayName("Should return failure with E-999 when CommitAsync fails")]
    public async Task ShouldReturnFailureWithE999WhenCommitFails()
    {
        // Given: the atomic write store returns failure
        AtomicWriteStoreMock
            .Setup(x => x.CommitAsync(It.IsAny<TransactionalWriteBatch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>(DomainErrors.GenericError));

        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: result is failure with E-999
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when CommitAsync fails but got success");
        Assert.That(result.Error, Is.EqualTo(DomainErrors.GenericError),
            $"Expected '{DomainErrors.GenericError}' but got '{result.Error}'");
    }

    #endregion

    #region LinkOrder Verification

    [Test]
    [DisplayName("Should call LinkOrder on the PaymentAttemptEntity before committing the batch")]
    public async Task ShouldCallLinkOrderOnPaymentAttemptEntityBeforeCommit()
    {
        // Given: the payments port returns a payment attempt entity
        PaymentAttemptEntity capturedAttempt = BuildValidPaymentAttemptEntity();

        PaymentsPortMock
            .Setup(x => x.StartPaymentProcessAsync(It.IsAny<CheckoutOrderModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(capturedAttempt));

        CreateOrderCommand command = BuildValidCommand();

        // When: the handler processes the command
        Result<OrderCheckoutModel> result = await Handler.Handle(command, CancellationToken.None);

        // Then: the payment attempt has OrderId set (LinkOrder was called before CommitAsync)
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(capturedAttempt.OrderId, Is.Not.Null.And.Not.Empty,
            "Expected OrderId to be set on the payment attempt after LinkOrder is called");
        Assert.That(capturedAttempt.UserId, Is.EqualTo(FakeUserId),
            $"Expected UserId '{FakeUserId}' on payment attempt after LinkOrder but got '{capturedAttempt.UserId}'");
    }

    #endregion
}

