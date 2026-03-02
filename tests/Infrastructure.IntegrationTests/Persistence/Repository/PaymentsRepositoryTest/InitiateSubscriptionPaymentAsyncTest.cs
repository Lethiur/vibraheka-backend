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

        // When
        Result<SubscriptionCheckoutSessionEntity> result = await _repository.InitiateSubscriptionPaymentAsync(user, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Url, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.Url.StartsWith("https://checkout.stripe.com/"), Is.True);
        Assert.That(result.Value.PaymentSessionID, Is.Not.Null.And.Not.Empty);
    }
}
