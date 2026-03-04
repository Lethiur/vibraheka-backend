using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

[TestFixture]
public class GetSubscriptionDetailsForUserTest : GenericSubscriptionRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReturnSubscriptionForUserWhenItExists()
    {
        // Given: un registro de suscripcion existente para el usuario.
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

        // When: se consulta la suscripcion por el user id.
        Result<SubscriptionEntity> result = await _repository.GetSubscriptionDetailsForUser(userId, CancellationToken.None);

        // Then: debe devolverse el registro persistido.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(userId));
        Assert.That(result.Value.SubscriptionID, Is.EqualTo(entity.SubscriptionID));
    }

    [Test]
    public async Task ShouldReturnNoSubscriptionFoundWhenUserHasNoSubscription()
    {
        // Given: un user id sin suscripciones registradas.
        string userIdWithoutSubscription = Guid.NewGuid().ToString();

        // When: se consulta la suscripcion para ese usuario.
        Result<SubscriptionEntity> result =
            await _repository.GetSubscriptionDetailsForUser(userIdWithoutSubscription, CancellationToken.None);

        // Then: debe mapear al error funcional de no suscripcion encontrada.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }
}
