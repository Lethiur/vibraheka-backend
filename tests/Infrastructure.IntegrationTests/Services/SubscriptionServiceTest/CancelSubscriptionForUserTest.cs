using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class CancelSubscriptionForUserTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldMarkSubscriptionAsToBeCancelled()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        await _subscriptionRepository.SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Active,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        }, CancellationToken.None);

        // When
        Result<Unit> cancelResult = await _service.CancelSubscriptionForUser(userId, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then
        Assert.That(cancelResult.IsSuccess, Is.True);
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.ToBeCancelled));
    }
}
