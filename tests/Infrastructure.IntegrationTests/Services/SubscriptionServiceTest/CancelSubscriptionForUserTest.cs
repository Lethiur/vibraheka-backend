using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class CancelSubscriptionForUserTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldMarkSubscriptionAsToBeCancelled()
    {
        // Given: una suscripcion activa existente para el usuario.
        string userId = Guid.NewGuid().ToString();
        await _subscriptionRepository.SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Active,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        }, CancellationToken.None);

        // When: se cancela la suscripcion del usuario.
        Result<Unit> cancelResult = await _service.CancelSubscriptionForUser(userId, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then: debe quedar marcada como ToBeCancelled.
        Assert.That(cancelResult.IsSuccess, Is.True);
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.ToBeCancelled));
    }

    [Test]
    public async Task ShouldFailWhenSubscriptionDoesNotExist()
    {
        // Given: un usuario sin suscripcion persistida.
        string userIdWithoutSubscription = Guid.NewGuid().ToString();

        // When: se intenta cancelar la suscripcion inexistente.
        Result<Unit> cancelResult = await _service.CancelSubscriptionForUser(userIdWithoutSubscription, CancellationToken.None);

        // Then: debe devolverse el error de no suscripcion encontrada.
        Assert.That(cancelResult.IsFailure, Is.True);
        Assert.That(cancelResult.Error, Is.EqualTo(SubscriptionErrors.NoSubscriptionFound));
    }
}
