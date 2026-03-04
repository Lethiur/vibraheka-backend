using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SubscriptionServiceTest;

[TestFixture]
public class CreateSubscriptionTest : GenericSubscriptionServiceIntegrationTest
{
    [Test]
    public async Task ShouldCreateSubscriptionWhenUserHasNoSubscription()
    {
        // Given: un usuario sin suscripcion previa y un checkout session valido.
        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.integration.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };

        UserEntity user = new()
        {
            Id = Guid.NewGuid().ToString(),
            CustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            CreatedBy = "integration-test"
        };

        // When: se crea la suscripcion para el usuario.
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then: debe persistirse una suscripcion en estado Created.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(user.Id));
        Assert.That(result.Value.ExternalCustomerID, Is.EqualTo(user.CustomerID));
        Assert.That(result.Value.CheckoutSessionUrl, Is.EqualTo(checkoutSession.Url));
        Assert.That(result.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));
    }

    [Test]
    public async Task ShouldResetCancelledSubscriptionToCreatedAndPending()
    {
        // Given: una suscripcion existente en estado Cancelled para el usuario.
        string userId = Guid.NewGuid().ToString();
        SubscriptionEntity existingSubscription = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Cancelled,
            Status = OrderStatus.Cancelled,
            StartDate = DateTimeOffset.UtcNow.AddDays(-10),
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };
        await _subscriptionRepository.SaveSubscriptionAsync(existingSubscription, CancellationToken.None);

        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.integration.reset.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(2),
        };

        UserEntity user = new()
        {
            Id = userId,
            CustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            CreatedBy = "integration-test"
        };

        // When: se crea nuevamente la suscripcion para ese usuario.
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then: debe reutilizarse y resetearse a Created/Pending con datos de checkout nuevos.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Created));
        Assert.That(result.Value.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(result.Value.CheckoutSessionUrl, Is.EqualTo(checkoutSession.Url));
        Assert.That(result.Value.CheckoutSessionExpiresAt, Is.EqualTo(checkoutSession.ExpiresAt));
        Assert.That(result.Value.ExternalCustomerID, Is.EqualTo(user.CustomerID));
    }

    [Test]
    public async Task ShouldKeepExistingSubscriptionWhenItIsNotCancelled()
    {
        // Given: una suscripcion existente que no esta cancelada.
        string userId = Guid.NewGuid().ToString();
        SubscriptionEntity existingSubscription = new()
        {
            SubscriptionID = Guid.NewGuid().ToString(),
            UserID = userId,
            ExternalSubscriptionID = "sub_test_" + Guid.NewGuid().ToString("N"),
            ExternalSubscriptionItemID = _stripeConfig.SubscriptionID,
            ExternalCustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            SubscriptionStatus = SubscriptionStatus.Active,
            Status = OrderStatus.OrderPayed,
            CheckoutSessionUrl = "https://checkout.integration.previous.test",
            Created = DateTime.UtcNow,
            CreatedBy = "integration-test"
        };
        await _subscriptionRepository.SaveSubscriptionAsync(existingSubscription, CancellationToken.None);

        SubscriptionCheckoutSessionEntity checkoutSession = new()
        {
            Url = "https://checkout.integration.new.test",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };
        UserEntity user = new()
        {
            Id = userId,
            CustomerID = "cus_test_" + Guid.NewGuid().ToString("N"),
            CreatedBy = "integration-test"
        };

        // When: se intenta crear una suscripcion para el mismo usuario.
        Result<SubscriptionEntity> result = await _service.CreateSubscription(user, checkoutSession, CancellationToken.None);

        // Then: debe retornarse la suscripcion existente sin resetear estados.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.SubscriptionID, Is.EqualTo(existingSubscription.SubscriptionID));
        Assert.That(result.Value.SubscriptionStatus, Is.EqualTo(SubscriptionStatus.Active));
        Assert.That(result.Value.Status, Is.EqualTo(OrderStatus.OrderPayed));
        Assert.That(result.Value.CheckoutSessionUrl, Is.EqualTo(existingSubscription.CheckoutSessionUrl));
    }
}
