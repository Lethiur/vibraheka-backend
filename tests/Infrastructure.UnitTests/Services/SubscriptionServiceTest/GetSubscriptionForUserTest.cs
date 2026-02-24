using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.SubscriptionServiceTest;

[TestFixture]
public class GetSubscriptionForUserTest : GenericSubscriptionServiceTest
{
    [Test]
    public async Task ShouldReturnSubscriptionFromRepository()
    {
        // Given
        SubscriptionEntity entity = new() { UserID = "user-1", SubscriptionID = "sub-1" };

        _subscriptionRepositoryMock.Setup(x => x.GetSubscriptionDetailsForUser("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(entity));

        // When
        Result<SubscriptionEntity> result = await _service.GetSubscriptionForUser("user-1", CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionID, Is.EqualTo("sub-1"));
    }
}
