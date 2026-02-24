using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class GetSubscriptionForUserTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldGetSubscriptionForExistingUser()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        await _subscriptionRepository.SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        }, CancellationToken.None);

        // When
        Result<SubscriptionEntity> result = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(userId));
    }
}
