using CSharpFunctionalExtensions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class GetSubscriptionPanelUrlAsyncTest : GenericPaymentsRepositoryTest
{
    [Test]
    public async Task ShouldReturnGeneralErrorWhenPayerIsNull()
    {
        // Given

        // When
        Result<string> result = await Repository.GetSubscriptionPanelUrlAsync(null!, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
