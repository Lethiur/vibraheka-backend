using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VibraHeka.Application.Subscriptions.Commands;
using VibraHeka.Application.Subscriptions.Commands.AddSubscription;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.UnitTests.Subscriptions.Commands.AddSubscription;

[TestFixture]
public class AddSubscriptionCommandHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IPaymentService> _paymentServiceMock;
    private Mock<ISubscriptionService> _subscriptionServiceMock;
    private Mock<ILogger<AddSubscriptionCommandHandler>> _loggerMock;
    private AddSubscriptionCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _loggerMock = new Mock<ILogger<AddSubscriptionCommandHandler>>();

        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-1");

        _handler = new AddSubscriptionCommandHandler(
            _currentUserServiceMock.Object,
            _paymentServiceMock.Object,
            _subscriptionServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task ShouldReturnCheckoutSessionWhenSubscriptionAndPaymentSucceed()
    {
        MockSequence sequence = new();
        SubscriptionContext preparation = new() { UserID = "user-1", ExternalCustomerID = "cus-1" };
        SubscriptionEntity subscription = new() { UserID = "user-1", SubscriptionID = "sub-1" };
        SubscriptionCheckoutSessionEntity session = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            PaymentSessionID = "cs_123",
            InternalPaymentID = "ref_123",
        };
        preparation.CheckoutSession = session;

        _paymentServiceMock.InSequence(sequence).Setup(x => x.PrepareSubscriptionAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(preparation));
        _subscriptionServiceMock.InSequence(sequence).Setup(x => x.CreateSubscription(preparation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(subscription));

        Result<SubscriptionCheckoutSessionEntity> result = await _handler.Handle(new AddSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Url, Is.EqualTo("https://checkout.test"));
        _paymentServiceMock.Verify(x => x.PrepareSubscriptionAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _subscriptionServiceMock.Verify(x => x.CreateSubscription(preparation, It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldRollbackStripeSessionWhenCreateSubscriptionFails()
    {
        MockSequence sequence = new();
        SubscriptionContext preparation = new() { UserID = "user-1", ExternalCustomerID = "cus-1" };
        SubscriptionCheckoutSessionEntity session = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            PaymentSessionID = "cs_rollback",
            InternalPaymentID = "ref_rollback",
        };
        preparation.CheckoutSession = session;

        _paymentServiceMock.InSequence(sequence).Setup(x => x.PrepareSubscriptionAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(preparation));
        _subscriptionServiceMock.InSequence(sequence).Setup(x => x.CreateSubscription(preparation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionEntity>("DB_ERROR"));
        _paymentServiceMock.InSequence(sequence).Setup(x => x.CancelSubscriptionPayment(session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        Result<SubscriptionCheckoutSessionEntity> result = await _handler.Handle(new AddSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
        _paymentServiceMock.Verify(x => x.PrepareSubscriptionAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _subscriptionServiceMock.Verify(x => x.CreateSubscription(preparation, It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.CancelSubscriptionPayment(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldMapErrorWhenPrepareSubscriptionFails()
    {
        _paymentServiceMock.Setup(x => x.PrepareSubscriptionAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionContext>("E-003"));

        Result<SubscriptionCheckoutSessionEntity> result = await _handler.Handle(new AddSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
        _paymentServiceMock.Verify(x => x.PrepareSubscriptionAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _subscriptionServiceMock.Verify(x => x.CreateSubscription(It.IsAny<SubscriptionContext>(), It.IsAny<CancellationToken>()), Times.Never);
        _paymentServiceMock.Verify(x => x.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
