using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class CancelSubscriptionForUserTest : GenericPaymentsRepositoryTest
{
    [Test]
    public async Task ShouldReturnGeneralErrorWhenSubscriptionEntityIsNull()
    {
        // Given

        // When
        Result<Unit> result = await Repository.CancelSubscriptionForUser(null!, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
