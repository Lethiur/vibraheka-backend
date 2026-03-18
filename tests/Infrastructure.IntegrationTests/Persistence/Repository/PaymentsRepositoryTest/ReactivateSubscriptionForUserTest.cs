using CSharpFunctionalExtensions;
using MediatR;
using Stripe;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class ReactivateSubscriptionForUserTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReactivateSubscriptionWhenItWasMarkedToCancelAtPeriodEnd()
    {
        // Given: una suscripcion que primero se marca para cancelacion al final del periodo.
        SubscriptionEntity subscription = await CreateStripeSubscriptionEntityAsync();
        Result<Unit> cancelResult = await _repository.CancelSubscriptionForUser(subscription, CancellationToken.None);
        Assert.That(cancelResult.IsSuccess, Is.True);

        // When: se solicita reactivar la suscripcion.
        Result<Unit> result = await _repository.ReactivateSubscriptionForUser(subscription, CancellationToken.None);

        // Then: Stripe debe dejar cancel_at_period_end en false.
        Assert.That(result.IsSuccess, Is.True);

        SubscriptionService service = new();
        Stripe.Subscription refreshed = await service.GetAsync(subscription.ExternalSubscriptionID, cancellationToken: CancellationToken.None);
        Assert.That(refreshed.CancelAtPeriodEnd, Is.False);
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenSubscriptionIdIsInvalid()
    {
        // Given: una suscripcion con id externo invalido.
        SubscriptionEntity entity = new()
        {
            UserID = Guid.NewGuid().ToString(),
            ExternalSubscriptionID = "sub_invalid_for_integration_test"
        };

        // When: se intenta reactivar una suscripcion inexistente.
        Result<Unit> result = await _repository.ReactivateSubscriptionForUser(entity, CancellationToken.None);

        // Then: debe retornarse error de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenSubscriptionEntityIsNull()
    {
        // Given: una entidad nula para reactivar.

        // When: se invoca reactivacion con parametro nulo.
        Result<Unit> result = await _repository.ReactivateSubscriptionForUser(null!, CancellationToken.None);

        // Then: debe retornarse error general de persistencia.
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
