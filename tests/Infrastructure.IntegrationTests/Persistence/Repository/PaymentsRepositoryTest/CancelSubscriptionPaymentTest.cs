using CSharpFunctionalExtensions;
using MediatR;
using Stripe.Checkout;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class CancelSubscriptionPaymentTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldExpireCheckoutSessionWhenSessionExists()
    {
        // Given: una sesion de checkout creada para un customer valido.
        UserEntity user = CreateValidUser();
        Result<string> customerResult = await _repository.RegisterCustomerAsync(user, CancellationToken.None);
        user.CustomerID = customerResult.Value;
        Result<SubscriptionCheckoutSessionEntity> checkoutSessionResult =
            await _repository.InitiateSubscriptionPaymentAsync(user, CancellationToken.None);

        // When: se solicita expirar la sesion de checkout.
        Result<Unit> result = await _repository.CancelSubscriptionPayment(checkoutSessionResult.Value, CancellationToken.None);

        // Then: la sesion debe quedar en estado expired.
        Assert.That(result.IsSuccess, Is.True);

        SessionService sessionService = new();
        Session session = await sessionService.GetAsync(checkoutSessionResult.Value.PaymentSessionID, cancellationToken: CancellationToken.None);
        Assert.That(session.Status, Is.EqualTo("expired"));
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenSessionIdIsInvalid()
    {
        // Given: una entidad con payment session id invalido.
        SubscriptionCheckoutSessionEntity entity = new()
        {
            PaymentSessionID = "cs_invalid_for_integration_test"
        };

        // When: se intenta expirar una sesion inexistente.
        Result<Unit> result = await _repository.CancelSubscriptionPayment(entity, CancellationToken.None);

        // Then: debe devolverse error de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenSessionEntityIsNull()
    {
        // Given: una entidad nula para expirar sesion.

        // When: se intenta cancelar con entidad nula.
        Result<Unit> result = await _repository.CancelSubscriptionPayment(null!, CancellationToken.None);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
