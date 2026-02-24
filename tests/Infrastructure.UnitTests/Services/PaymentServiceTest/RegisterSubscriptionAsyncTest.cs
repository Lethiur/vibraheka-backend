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
        // Given
        UserEntity user = new() { Id = "user-1", CustomerID = "", Email = "mail@test.com" };
        SubscriptionEntity subscription = new() { SubscriptionID = "sub-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.RegisterCustomerAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("cus-123"));
        _userRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(Result.Success("user-1"));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, subscription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://checkout.test"));

        // When
        Result<string> result = await _service.RegisterSubscriptionAsync("user-1", subscription, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://checkout.test"));
        Assert.That(user.CustomerID, Is.EqualTo("cus-123"));
        _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
    }

    [Test]
    public async Task ShouldSkipCustomerRegistrationWhenUserAlreadyHasCustomerId()
    {
        // Given
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-existing" };
        SubscriptionEntity subscription = new() { SubscriptionID = "sub-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, subscription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("https://checkout.test"));

        // When
        Result<string> result = await _service.RegisterSubscriptionAsync("user-1", subscription, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _paymentRepositoryMock.Verify(x => x.RegisterCustomerAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserEntity>()), Times.Never);
    }

    [Test]
    public async Task ShouldMapStripeErrorToSubscriptionErrorWhenPaymentFails()
    {
        // Given
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<SubscriptionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(InfrastructureSubscriptionErrors.StripeError));

        // When
        Result<string> result = await _service.RegisterSubscriptionAsync("user-1", new SubscriptionEntity(), CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }

    [Test]
    public async Task ShouldMapGenericPersistenceErrorToSubscriptionError()
    {
        // Given
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(user));
        _paymentRepositoryMock.Setup(x => x.InitiateSubscriptionPaymentAsync(user, It.IsAny<SubscriptionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<string>(GenericPersistenceErrors.GeneralError));

        // When
        Result<string> result = await _service.RegisterSubscriptionAsync("user-1", new SubscriptionEntity(), CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }
}
