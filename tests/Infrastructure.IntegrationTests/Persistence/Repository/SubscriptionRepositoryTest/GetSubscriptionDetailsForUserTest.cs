using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class GetSubscriptionDetailsForUserTest : GenericSubscriptionRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReturnSubscriptionForUserWhenItExists()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        SubscriptionEntity entity = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };

        await _repository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // When
        Result<SubscriptionEntity> result = await _repository.GetSubscriptionDetailsForUser(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(userId));
        Assert.That(result.Value.SubscriptionID, Is.EqualTo(entity.SubscriptionID));
    }
}
