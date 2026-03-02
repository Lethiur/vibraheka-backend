using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.SubscriptionServiceTest;

[TestFixture]
public class CreateSubscriptionTest : GenericSubscriptionServiceTest
{
    [Test]
    public async Task ShouldResetCancelledSubscriptionToCreatedAndPending()
    {
        // Given
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1" };
        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };
        SubscriptionEntity cancelled = new()
        {
            UserID = "user-1",
            SubscriptionStatus = SubscriptionStatus.Cancelled,
            Status = OrderStatus.Cancelled
        };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(cancelled));
        _subscriptionRepositoryMock.Setup(x => x.SaveSubscriptionAsync(cancelled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(cancelled));

        // When
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(cancelled.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));
        Assert.That(cancelled.Status, Is.EqualTo(OrderStatus.Pending));
        _subscriptionRepositoryMock.Verify(x => x.SaveSubscriptionAsync(cancelled, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ShouldCreateNewSubscriptionWhenNoneExists()
    {
        // Given
        UserEntity user = new() { Id = "user-1", CustomerID = "cus-1", CreatedBy = "creator" };
        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionEntity>(SubscriptionErrors.NoSubscriptionFound));
        _subscriptionRepositoryMock.Setup(x => x.SaveSubscriptionAsync(It.IsAny<SubscriptionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubscriptionEntity entity, CancellationToken _) => Result.Success(entity));

        // When
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo("user-1"));
        Assert.That(result.Value.ExternalCustomerID, Is.EqualTo("cus-1"));
        Assert.That(result.Value.ExternalSubscriptionItemID, Is.EqualTo("price-123"));
        Assert.That(result.Value.CheckoutSessionUrl, Is.EqualTo("https://checkout.test"));
        Assert.That(result.Value.CheckoutSessionExpiresAt, Is.EqualTo(checkoutSession.ExpiresAt));
        Assert.That(result.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));
        Assert.That(result.Value.Status, Is.EqualTo(OrderStatus.Pending));
    }

    [Test]
    public async Task ShouldPropagateFailureWhenCreateSubscriptionFailsForDifferentReason()
    {
        // Given
        UserEntity user = new() { Id = "user-1" };
        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubscriptionEntity>("ANY-ERROR"));

        // When
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("ANY-ERROR"));
    }
}
