using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Services.SubscriptionServiceTest;

[TestFixture]
public class ReactivateSubscriptionTest : GenericSubscriptionServiceTest
{
    [Test]
    public async Task ShouldReactivateSubscriptionWhenStatusIsToBeCancelled()
    {
        // Given
        SubscriptionEntity entity = new()
        {
            UserID = "user-1",
            SubscriptionStatus = SubscriptionStatus.ToBeCancelled,
            ExternalSubscriptionID = "ext-sub"
        };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));
        _subscriptionRepositoryMock.Setup(x => x.SaveSubscriptionAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));
        _paymentRepositoryMock.Setup(x => x.ReactivateSubscriptionForUser(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _service.ReactivateSubscription("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(entity.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Active));
    }

    [Test]
    public async Task ShouldFailReactivationWhenSubscriptionIsAlreadyActive()
    {
        // Given
        SubscriptionEntity entity = new() { UserID = "user-1", SubscriptionStatus = SubscriptionStatus.Active };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        // When
        Result<Unit> result = await _service.ReactivateSubscription("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.SubscriptionIsActive));
        _paymentRepositoryMock.Verify(x => x.ReactivateSubscriptionForUser(It.IsAny<SubscriptionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
