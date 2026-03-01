using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class InitiateSubscriptionPaymentAsyncTest : GenericPaymentsRepositoryTest
{
    [Test]
    public async Task ShouldReturnGeneralErrorWhenPayerIsNull()
    {
        // Given
        // When
        Result<SubscriptionCheckoutSessionEntity> result = await Repository.InitiateSubscriptionPaymentAsync(null!, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
