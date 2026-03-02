using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.SubscriptionServiceTest;

[TestFixture]
public class DeleteSubscriptionForUserTest : GenericSubscriptionServiceTest
{
    [Test]
    public async Task ShouldDeleteSubscriptionForUserWhenFound()
    {
        // Given
        SubscriptionEntity entity = new() { UserID = "user-1" };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));
        _subscriptionRepositoryMock.Setup(x => x.DeleteSubscriptionForUser(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // When
        Result<Unit> result = await _service.DeleteSubscriptionForUser("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        _subscriptionRepositoryMock.Verify(x => x.DeleteSubscriptionForUser(entity, It.IsAny<CancellationToken>()), Times.Once);
    }
}
