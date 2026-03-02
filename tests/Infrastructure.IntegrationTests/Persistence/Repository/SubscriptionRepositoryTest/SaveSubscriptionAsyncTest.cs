using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class SaveSubscriptionAsyncTest : GenericSubscriptionRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldPersistSubscriptionWhenSaveIsCalled()
    {
        // Given
        string subscriptionId = Guid.NewGuid().ToString();
        SubscriptionEntity entity = new()
        {
            SubscriptionID = subscriptionId,
            UserID = Guid.NewGuid().ToString(),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };

        // When
        Result<SubscriptionEntity> result = await _repository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionID, Is.EqualTo(subscriptionId));
    }
}
