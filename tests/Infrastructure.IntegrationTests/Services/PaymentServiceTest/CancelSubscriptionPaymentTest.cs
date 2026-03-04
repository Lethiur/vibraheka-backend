using CSharpFunctionalExtensions;
using MediatR;
using Stripe.Checkout;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class CancelSubscriptionPaymentTest : TestBase
{
    private PaymentService _paymentService;
    private IPaymentRepository _paymentRepository;
    private IUserRepository _userRepository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _userRepository = new UserRepository(CreateDynamoDBContext(), _configuration);
        _paymentRepository = new PaymentsRepository(_stripeConfig, CreateTestLogger<PaymentsRepository>());
        _paymentService = new PaymentService(_paymentRepository, _userRepository);
    }

    [Test]
    public async Task ShouldFailWhenSessionEntityIsNull()
    {
        // Given: una entidad nula para cancelar un checkout.

        // When: se invoca la cancelacion de pago con entidad nula.
        Result<Unit> result = await _paymentService.CancelSubscriptionPayment(null!, CancellationToken.None);

        // Then: debe fallar con el error funcional de suscripcion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }

    [Test]
    public async Task ShouldCancelPaymentSessionWhenCheckoutSessionExists()
    {
        // Given: una sesion de checkout valida creada para un usuario.
        UserEntity userEntity = CreateValidUser();
        Result<string> registerCustomerResult = await _paymentRepository.RegisterCustomerAsync(userEntity, CancellationToken.None);
        userEntity.CustomerID = registerCustomerResult.Value;
        Result<SubscriptionCheckoutSessionEntity> checkoutSessionResult =
            await _paymentRepository.InitiateSubscriptionPaymentAsync(userEntity, CancellationToken.None);

        // When: se solicita cancelar la sesion de checkout.
        Result<Unit> result =
            await _paymentService.CancelSubscriptionPayment(checkoutSessionResult.Value, CancellationToken.None);

        // Then: la sesion debe expirar correctamente en Stripe.
        Assert.That(result.IsSuccess, Is.True);

        SessionService sessionService = new();
        Session session = await sessionService.GetAsync(checkoutSessionResult.Value.PaymentSessionID, cancellationToken: CancellationToken.None);
        Assert.That(session.Status, Is.EqualTo("expired"));
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenSessionIdIsInvalid()
    {
        // Given: una entidad con payment session id invalido para Stripe.
        SubscriptionCheckoutSessionEntity entity = new()
        {
            PaymentSessionID = "cs_invalid_for_integration_test"
        };

        // When: se intenta cancelar esa sesion inexistente.
        Result<Unit> result = await _paymentService.CancelSubscriptionPayment(entity, CancellationToken.None);

        // Then: debe devolverse el error tecnico de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }
}
