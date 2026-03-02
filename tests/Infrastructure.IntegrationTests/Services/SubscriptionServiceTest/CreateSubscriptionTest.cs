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
        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.integration.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };

        UserEntity user = new()
        {
            Id = Guid.NewGuid().ToString(),
            CustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            CreatedBy = "integration-test"
        };

        // When
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(user.Id));
        Assert.That(result.Value.ExternalCustomerID, Is.EqualTo(user.CustomerID));
        Assert.That(result.Value.CheckoutSessionUrl, Is.EqualTo(checkoutSession.Url));
        Assert.That(result.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));
    }
}
