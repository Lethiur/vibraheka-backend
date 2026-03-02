using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.SubscriptionServiceTest;

[TestFixture]
public class CancelSubscriptionForUserTest : GenericSubscriptionServiceTest
{
    [Test]
    public async Task ShouldMarkSubscriptionToBeCancelledAndCallPaymentRepository()
    {
        // Given
        SubscriptionEntity entity = new() { UserID = "user-1", ExternalSubscriptionID = "ext-sub" };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));
        _subscriptionRepositoryMock.Setup(x => x.SaveSubscriptionAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));
        _paymentRepositoryMock.Setup(x => x.CancelSubscriptionForUser(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _service.CancelSubscriptionForUser("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(entity.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.ToBeCancelled));
        _paymentRepositoryMock.Verify(x => x.CancelSubscriptionForUser(entity, It.IsAny<CancellationToken>()), Times.Once);
    }
}
