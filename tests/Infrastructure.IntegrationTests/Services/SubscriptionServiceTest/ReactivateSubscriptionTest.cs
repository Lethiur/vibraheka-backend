using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class ReactivateSubscriptionTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldReactivateSubscriptionWhenItIsMarkedToBeCancelled()
    {
        // Given: una suscripcion pendiente de cancelacion y elegible para reactivarse.
        string userId = Guid.NewGuid().ToString();
        await SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.ToBeCancelled,
            Status = OrderStatus.Pending,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        });

        // When: se ejecuta la reactivacion para el usuario.
        Result<Unit> reactivateResult = await _service.ReactivateSubscription(userId, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then: debe quedar reactivada como activa.
        Assert.That(reactivateResult.IsSuccess, Is.True);
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Active));
    }

    [Test]
    public async Task ShouldFailWhenSubscriptionIsNotMarkedAsToBeCancelled()
    {
        // Given: una suscripcion ya activa (no cumple el primer ensure).
        string userId = Guid.NewGuid().ToString();
        await SaveSubscriptionAsync(new SubscriptionEntity
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
        });

        // When: se intenta reactivar una suscripcion que no esta en ToBeCancelled.
        Result<Unit> reactivateResult = await _service.ReactivateSubscription(userId, CancellationToken.None);

        // Then: debe fallar con SubscriptionIsActive.
        Assert.That(reactivateResult.IsFailure, Is.True);
        Assert.That(reactivateResult.Error, Is.EqualTo(SubscriptionErrors.SubscriptionIsActive));
    }

    [Test]
    public async Task ShouldFailWhenOrderStatusIsPaymentFailed()
    {
        // Given: una suscripcion en ToBeCancelled pero con estado PaymentFailed.
        string userId = Guid.NewGuid().ToString();
        await SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.ToBeCancelled,
            Status = OrderStatus.PaymentFailed,
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        });

        // When: se intenta reactivar con un estado que el segundo ensure bloquea.
        Result<Unit> reactivateResult = await _service.ReactivateSubscription(userId, CancellationToken.None);

        // Then: debe fallar con SubscriptionIsCancelled.
        Assert.That(reactivateResult.IsFailure, Is.True);
        Assert.That(reactivateResult.Error, Is.EqualTo(SubscriptionErrors.SubscriptionIsCancelled));
    }

    [Test]
    public async Task ShouldSetTrialingWhenReactivatingDelayedSubscriptionWithFutureStartDate()
    {
        // Given: una suscripcion ToBeCancelled con orden demorada y start date futura.
        string userId = Guid.NewGuid().ToString();
        await SaveSubscriptionAsync(new SubscriptionEntity
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.ToBeCancelled,
            Status = OrderStatus.OrderDelayed,
            StartDate = DateTimeOffset.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        });

        // When: se reactiva la suscripcion demorada.
        Result<Unit> reactivateResult = await _service.ReactivateSubscription(userId, CancellationToken.None);
        Result<SubscriptionEntity> getResult = await _service.GetSubscriptionForUser(userId, CancellationToken.None);

        // Then: debe pasar de ToBeCancelled a Trialing.
        Assert.That(reactivateResult.IsSuccess, Is.True);
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Trialing));
    }

    private Task<Result<SubscriptionEntity>> SaveSubscriptionAsync(SubscriptionEntity entity)
    {
        return _subscriptionRepository.SaveSubscriptionAsync(entity, CancellationToken.None);
    }
}
