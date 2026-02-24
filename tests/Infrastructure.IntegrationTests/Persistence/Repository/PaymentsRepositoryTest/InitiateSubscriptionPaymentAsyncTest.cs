using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class InitiateSubscriptionPaymentAsyncTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldCreateCheckoutSessionWhenDataIsValid()
    {
        // Given
        UserEntity user = CreateValidUser();
        Result<string> customerResult = await _repository.RegisterCustomerAsync(user, CancellationToken.None);
        user.CustomerID = customerResult.Value;

        SubscriptionEntity subscription = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = user.Id,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending
        };

        // When
        Result<string> result = await _repository.InitiateSubscriptionPaymentAsync(user, subscription, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.StartsWith("https://checkout.stripe.com/"), Is.True);
    }
}
