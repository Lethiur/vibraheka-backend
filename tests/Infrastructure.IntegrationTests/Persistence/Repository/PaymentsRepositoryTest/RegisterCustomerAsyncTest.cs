using CSharpFunctionalExtensions;
using Stripe;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class RegisterCustomerAsyncTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldRegisterCustomerInStripe()
    {
        // Given: un usuario valido con datos minimos para crear customer.
        UserEntity user = CreateValidUser();

        // When: se registra el usuario como customer en Stripe.
        Result<string> result = await _repository.RegisterCustomerAsync(user, CancellationToken.None);

        // Then: debe devolverse un customer id valido.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.StartsWith("cus_"), Is.True);
    }

    [Test]
    [NonParallelizable]
    public async Task ShouldReturnStripeErrorWhenApiKeyIsInvalid()
    {
        // Given: una API key invalida para forzar error de autenticacion en Stripe.
        UserEntity user = CreateValidUser();
        string previousApiKey = StripeConfiguration.ApiKey;
        StripeConfiguration.ApiKey = "sk_test_invalid_key_for_integration_test";

        try
        {
            // When: se intenta registrar customer con credenciales invalidas.
            Result<string> result = await _repository.RegisterCustomerAsync(user, CancellationToken.None);

            // Then: debe devolverse el error de Stripe.
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
        }
        finally
        {
            StripeConfiguration.ApiKey = previousApiKey;
        }
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenUserIsNull()
    {
        // Given: un usuario nulo para registrar.

        // When: se invoca el registro sin entidad de usuario.
        Result<string> result = await _repository.RegisterCustomerAsync(null!, CancellationToken.None);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
