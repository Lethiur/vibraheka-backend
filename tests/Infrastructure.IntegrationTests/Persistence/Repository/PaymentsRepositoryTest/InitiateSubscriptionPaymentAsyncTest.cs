using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class InitiateSubscriptionPaymentAsyncTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldCreateCheckoutSessionWhenDataIsValid()
    {
        // Given: un usuario con customer id valido en Stripe.
        UserEntity user = CreateValidUser();
        Result<string> customerResult = await _repository.RegisterCustomerAsync(user, CancellationToken.None);
        user.CustomerID = customerResult.Value;

        // When: se inicia el pago de suscripcion.
        Result<SubscriptionCheckoutSessionEntity> result = await _repository.InitiateSubscriptionPaymentAsync(user, CancellationToken.None);

        // Then: debe crearse una sesion de checkout valida.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Url, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.Url.StartsWith("https://checkout.stripe.com/"), Is.True);
        Assert.That(result.Value.PaymentSessionID, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenCustomerIdIsInvalid()
    {
        // Given: un usuario con customer id invalido para Stripe.
        UserEntity user = CreateValidUser();
        user.CustomerID = "cus_invalid_for_integration_test";

        // When: se intenta iniciar el pago para ese customer inexistente.
        Result<SubscriptionCheckoutSessionEntity> result = await _repository.InitiateSubscriptionPaymentAsync(user, CancellationToken.None);

        // Then: debe devolverse error de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenPayerIsNull()
    {
        // Given: un payer nulo.

        // When: se intenta iniciar el pago sin entidad de usuario.
        Result<SubscriptionCheckoutSessionEntity> result = await _repository.InitiateSubscriptionPaymentAsync(null!, CancellationToken.None);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
