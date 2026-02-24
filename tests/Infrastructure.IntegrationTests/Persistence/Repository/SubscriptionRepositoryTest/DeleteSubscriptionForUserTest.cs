using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class DeleteSubscriptionForUserTest : GenericSubscriptionRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldDeleteSubscriptionWhenItExists()
    {
        // Given
        string userId = Guid.NewGuid().ToString();
        SubscriptionEntity entity = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Created,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };

        await _repository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // When
        Result<Unit> deleteResult = await _repository.DeleteSubscriptionForUser(entity, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _repository.GetSubscriptionDetailsForUser(userId, CancellationToken.None);

        // Then
        Assert.That(deleteResult.IsSuccess, Is.True);
        Assert.That(getResult.IsFailure, Is.True);
    }
}
