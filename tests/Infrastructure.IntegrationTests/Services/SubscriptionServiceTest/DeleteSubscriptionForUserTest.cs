using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class DeleteSubscriptionForUserTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldDeleteSubscriptionForExistingUser()
    {
        // Given: una suscripcion existente para el usuario.
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

        await _subscriptionRepository.SaveSubscriptionAsync(entity, CancellationToken.None);

        // When: se elimina la suscripcion del usuario.
        Result<Unit> deleteResult = await _service.DeleteSubscriptionForUser(userId, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then: la eliminacion debe ser exitosa y ya no debe existir la suscripcion.
        Assert.That(deleteResult.IsSuccess, Is.True);
        Assert.That(getResult.IsFailure, Is.True);
    }

    [Test]
    public async Task ShouldFailWhenDeletingNonExistingSubscription()
    {
        // Given: un usuario sin suscripcion.
        string userIdWithoutSubscription = Guid.NewGuid().ToString();

        // When: se intenta eliminar una suscripcion inexistente.
        Result<Unit> deleteResult = await _service.DeleteSubscriptionForUser(userIdWithoutSubscription, CancellationToken.None);

        // Then: debe fallar con error de no suscripcion encontrada.
        Assert.That(deleteResult.IsFailure, Is.True);
        Assert.That(deleteResult.Error, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }
}
