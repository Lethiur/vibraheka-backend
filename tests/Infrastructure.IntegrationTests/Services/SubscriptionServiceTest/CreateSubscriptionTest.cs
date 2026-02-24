using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class CreateSubscriptionTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldCreateSubscriptionWhenUserHasNoSubscription()
    {
        // Given
        UserEntity user = new()
        {
            Id = Guid.NewGuid().ToString(),
            CustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            CreatedBy = "integration-test"
        };

        // When
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(user.Id));
        Assert.That(result.Value.ExternalCustomerID, Is.EqualTo(user.CustomerID));
        Assert.That(result.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));
    }
}
