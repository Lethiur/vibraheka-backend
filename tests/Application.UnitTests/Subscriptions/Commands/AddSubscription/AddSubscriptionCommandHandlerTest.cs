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
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.UnitTests.Subscriptions.Commands.AddSubscription;

[TestFixture]
public class AddSubscriptionCommandHandlerTest
{
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<IPaymentService> _paymentServiceMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ISubscriptionService> _subscriptionServiceMock;
    private Mock<ILogger<AddSubscriptionCommandHandler>> _loggerMock;
    private AddSubscriptionCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _userServiceMock = new Mock<IUserService>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _loggerMock = new Mock<ILogger<AddSubscriptionCommandHandler>>();

        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-1");

        _handler = new AddSubscriptionCommandHandler(
            _currentUserServiceMock.Object,
            _paymentServiceMock.Object,
            _userServiceMock.Object,
            _subscriptionServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task ShouldReturnCheckoutSessionWhenSubscriptionAndPaymentSucceed()
    {
        MockSequence sequence = new();
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };
        SubscriptionEntity subscription = new() { UserID = "user-1", SubscriptionID = "sub-1" };
        SubscriptionCheckoutSessionEntity session = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            PaymentSessionID = "cs_123",
            InternalPaymentID = "ref_123",
        };

        _userServiceMock.InSequence(sequence).Setup(x => x.GetUserByID("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentServiceMock.InSequence(sequence).Setup(x => x.RegisterSubscriptionAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(session));
        _subscriptionServiceMock.InSequence(sequence).Setup(x => x.CreateSubscription(user, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(subscription));

        Result<SubscriptionCheckoutSessionEntity> result = await _handler.Handle(new AddSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Url, Is.EqualTo("https://checkout.test"));
        _userServiceMock.Verify(x => x.GetUserByID("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.RegisterSubscriptionAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _subscriptionServiceMock.Verify(x => x.CreateSubscription(user, session, It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldRollbackStripeSessionWhenCreateSubscriptionFails()
    {
        MockSequence sequence = new();
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };
        SubscriptionCheckoutSessionEntity session = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
            PaymentSessionID = "cs_rollback",
            InternalPaymentID = "ref_rollback",
        };

        _userServiceMock.InSequence(sequence).Setup(x => x.GetUserByID("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentServiceMock.InSequence(sequence).Setup(x => x.RegisterSubscriptionAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(session));
        _subscriptionServiceMock.InSequence(sequence).Setup(x => x.CreateSubscription(user, session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionEntity>("DB_ERROR"));
        _paymentServiceMock.InSequence(sequence).Setup(x => x.CancelSubscriptionPayment(session, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        Result<SubscriptionCheckoutSessionEntity> result = await _handler.Handle(new AddSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
        _userServiceMock.Verify(x => x.GetUserByID("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.RegisterSubscriptionAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _subscriptionServiceMock.Verify(x => x.CreateSubscription(user, session, It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.CancelSubscriptionPayment(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldMapErrorWhenUserLookupFails()
    {
        _userServiceMock.Setup(x => x.GetUserByID("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserEntity>("E-003"));

        Result<SubscriptionCheckoutSessionEntity> result = await _handler.Handle(new AddSubscriptionCommand(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
        _userServiceMock.Verify(x => x.GetUserByID("user-1", It.IsAny<CancellationToken>()), Times.Once);
        _paymentServiceMock.Verify(x => x.RegisterSubscriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _subscriptionServiceMock.Verify(x => x.CreateSubscription(It.IsAny<UserEntity>(), It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _paymentServiceMock.Verify(x => x.CancelSubscriptionPayment(It.IsAny<SubscriptionCheckoutSessionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
