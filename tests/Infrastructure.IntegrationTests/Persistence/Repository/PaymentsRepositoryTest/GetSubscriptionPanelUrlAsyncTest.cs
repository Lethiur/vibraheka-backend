using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.PaymentsRepositoryTest;

[TestFixture]
public class GetSubscriptionPanelUrlAsyncTest : GenericPaymentsRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReturnBillingPortalUrlWhenCustomerExists()
    {
        // Given: un usuario con customer id valido en Stripe.
        UserEntity user = CreateValidUser();
        Result<string> customerResult = await _repository.RegisterCustomerAsync(user, CancellationToken.None);
        user.CustomerID = customerResult.Value;

        // When: se solicita la URL del portal de suscripcion.
        Result<string> result = await _repository.GetSubscriptionPanelUrlAsync(user, CancellationToken.None);

        // Then: debe devolverse una URL valida del billing portal.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.StartsWith("https://billing.stripe.com/"), Is.True);
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenCustomerIdIsInvalid()
    {
        // Given: un usuario con customer id invalido para Stripe.
        UserEntity user = CreateValidUser();
        user.CustomerID = "cus_invalid_for_integration_test";

        // When: se consulta el portal con customer inexistente.
        Result<string> result = await _repository.GetSubscriptionPanelUrlAsync(user, CancellationToken.None);

        // Then: debe devolverse error de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }

    [Test]
    public async Task ShouldReturnGeneralErrorWhenUserEntityIsNull()
    {
        // Given: una entidad de usuario nula.

        // When: se solicita el portal sin usuario.
        Result<string> result = await _repository.GetSubscriptionPanelUrlAsync(null!, CancellationToken.None);

        // Then: debe devolverse error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
