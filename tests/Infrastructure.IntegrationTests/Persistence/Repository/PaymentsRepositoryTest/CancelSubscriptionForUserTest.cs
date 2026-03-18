using CSharpFunctionalExtensions;
using MediatR;
using Stripe;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class CancelSubscriptionForUserTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldCancelSubscriptionAtPeriodEndWhenSubscriptionExists()
    {
        // Given: una suscripcion real en Stripe asociada a un customer valido.
        SubscriptionEntity subscription = await CreateStripeSubscriptionEntityAsync();

        // When: se solicita cancelar la suscripcion al cierre del periodo.
        Result<Unit> result = await _repository.CancelSubscriptionForUser(subscription, CancellationToken.None);

        // Then: debe marcarse cancelacion al final del periodo en Stripe.
        Assert.That(result.IsSuccess, Is.True);

        SubscriptionService service = new();
        Stripe.Subscription refreshed = await service.GetAsync(subscription.ExternalSubscriptionID, cancellationToken: CancellationToken.None);
        Assert.That(refreshed.CancelAtPeriodEnd, Is.True);
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenSubscriptionIdIsInvalid()
    {
        // Given: una entidad con external subscription id invalido.
        SubscriptionEntity entity = new()
        {
            UserID = Guid.NewGuid().ToString(),
            ExternalSubscriptionID = "sub_invalid_for_integration_test"
        };

        // When: se intenta cancelar una suscripcion inexistente.
        Result<Unit> result = await _repository.CancelSubscriptionForUser(entity, CancellationToken.None);

        // Then: debe devolverse error de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenSubscriptionEntityIsNull()
    {
        // Given: una entidad nula para cancelar.

        // When: se invoca cancelacion con parametro nulo.
        Result<Unit> result = await _repository.CancelSubscriptionForUser(null!, CancellationToken.None);

        // Then: debe devolverse el error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }

    private async Task<SubscriptionEntity> CreateStripeSubscriptionEntityAsync()
    {
        UserEntity user = CreateValidUser();
        Result<string> customerResult = await _repository.RegisterCustomerAsync(user, CancellationToken.None);

        SubscriptionService service = new();
        Subscription stripeSubscription = await service.CreateAsync(new SubscriptionCreateOptions
        {
            Customer = customerResult.Value,
            Items = [new SubscriptionItemOptions { Price = _stripeConfig.SubscriptionID }],
            TrialPeriodDays = Math.Max(_stripeConfig.TrialPeriodInDays, 1)
        }, cancellationToken: CancellationToken.None);

        return new SubscriptionEntity
        {
            UserID = user.Id,
            ExternalSubscriptionID = stripeSubscription.Id,
            ExternalCustomerID = customerResult.Value,
            ExternalSubscriptionItemID = stripeSubscription.Items.Data.FirstOrDefault()?.Id ?? string.Empty
        };
    }
}
