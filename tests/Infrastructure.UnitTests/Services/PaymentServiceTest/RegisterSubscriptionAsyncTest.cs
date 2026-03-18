using System.Data;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.PaymentServiceTest;

[TestFixture]
public class RegisterSubscriptionAsyncTest : GenericPaymentServiceTest
{
    [Test]
    public async Task ShouldRegisterCustomerAndPersistUserWhenCustomerIdIsMissing()
    {
        UserEntity user = new() { Id = "user-1", CustomerID = "", Email = "mail@test.com" };
        SubscriptionCheckoutSessionEntity session = new() { Url = "https://checkout.test", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1) };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.RegisterCustomerAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("cus-123"));
        _userRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(Result.Success("user-1"));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(session));

        Result<SubscriptionCheckoutSessionEntity> result = await _service.RegisterSubscriptionAsync("user-1", CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Url, Is.EqualTo("https://checkout.test"));
        Assert.That(user.CustomerID, Is.EqualTo("cus-123"));
        _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
    }

    [Test]
    public async Task ShouldSkipCustomerRegistrationWhenUserAlreadyHasCustomerId()
    {
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-existing" };
        SubscriptionCheckoutSessionEntity session = new() { Url = "https://checkout.test", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1) };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(session));

        Result<SubscriptionCheckoutSessionEntity> result = await _service.RegisterSubscriptionAsync("user-1", CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _paymentRepositoryMock.Verify(x => x.RegisterCustomerAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task ShouldMapStripeErrorToSubscriptionErrorWhenPaymentFails()
    {
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionCheckoutSessionEntity>(InfrastructureSubscriptionErrors.StripeError));

        Result<SubscriptionCheckoutSessionEntity> result = await _service.RegisterSubscriptionAsync("user-1", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }

    [Test]
    public async Task ShouldMapGenericPersistenceErrorToSubscriptionError()
    {
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionCheckoutSessionEntity>(GenericPersistenceErrors.GeneralError));

        Result<SubscriptionCheckoutSessionEntity> result = await _service.RegisterSubscriptionAsync("user-1", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }

    [Test]
    public async Task ShouldHandleExceptionsFromRepository()
    {
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DataException("Database error"));

        Result<SubscriptionCheckoutSessionEntity> result = await _service.RegisterSubscriptionAsync("user-1", CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Database error"));
    }
}
