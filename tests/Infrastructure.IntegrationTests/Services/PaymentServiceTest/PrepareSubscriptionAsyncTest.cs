using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class PrepareSubscriptionAsyncTest : TestBase
{
    private PaymentService _paymentService;
    private IPaymentRepository _paymentRepository;
    private IUserRepository _userRepository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _userRepository = new UserRepository(CreateDynamoDBContext(), _configuration);
        _paymentRepository = new PaymentsRepository(
            _stripeConfig,
            _configuration,
            CreateSystemsManagementClient(),
            CreateTestLogger<PaymentsRepository>());
        _paymentService = new PaymentService(_paymentRepository, _userRepository);
    }

    [Test]
    public async Task ShouldPrepareContextAndPersistCustomerIdWhenUserHasNoCustomerId()
    {
        // Given: un usuario valido persistido sin customer id en Stripe.
        UserProfileEntity userProfileEntity = CreateValidUser();
        await _userRepository.AddAsync(userProfileEntity);

        // When: se prepara la suscripcion para ese usuario.
        Result<SubscriptionContext> result = await _paymentService.PrepareSubscriptionAsync(userProfileEntity.Id, CancellationToken.None);

        // Then: debe crear contexto de checkout y persistir customer id.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(userProfileEntity.Id));
        Assert.That(result.Value.ExternalCustomerID, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.CheckoutSession.Url, Is.Not.Null.And.Not.Empty);

        Result<UserProfileEntity> persistedUser = await _userRepository.GetByIdAsync(userProfileEntity.Id, CancellationToken.None);
        Assert.That(persistedUser.IsSuccess, Is.True);
        Assert.That(persistedUser.Value.CustomerID, Is.EqualTo(result.Value.ExternalCustomerID));
    }

    [Test]
    public async Task ShouldReuseExistingCustomerIdWhenUserAlreadyHasOne()
    {
        // Given: un usuario con customer id ya registrado previamente en Stripe.
        UserProfileEntity userProfileEntity = CreateValidUser();
        Result<string> registerCustomerResult = await _paymentRepository.RegisterCustomerAsync(userProfileEntity, CancellationToken.None);
        userProfileEntity.CustomerID = registerCustomerResult.Value;
        await _userRepository.AddAsync(userProfileEntity);

        // When: se prepara la suscripcion para el usuario ya asociado.
        Result<SubscriptionContext> result = await _paymentService.PrepareSubscriptionAsync(userProfileEntity.Id, CancellationToken.None);

        // Then: debe usar el customer id existente y devolver sesion valida.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ExternalCustomerID, Is.EqualTo(userProfileEntity.CustomerID));
        Assert.That(result.Value.CheckoutSession.PaymentSessionID, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ShouldMapStripeErrorToSubscriptionErrorWhenCheckoutCreationFails()
    {
        // Given: un usuario persistido con customer id invalido para forzar error Stripe.
        UserProfileEntity userProfileEntity = CreateValidUser();
        userProfileEntity.CustomerID = "cus_invalid_for_integration_test";
        await _userRepository.AddAsync(userProfileEntity);

        // When: se prepara la suscripcion usando ese customer id invalido.
        Result<SubscriptionContext> result = await _paymentService.PrepareSubscriptionAsync(userProfileEntity.Id, CancellationToken.None);

        // Then: debe mapear al error funcional de suscripcion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SubscriptionErrors.ErrorWhileSubscribing));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Test]
    public async Task ShouldReturnInvalidUserIdWhenIdIsInvalid(string? invalidUserId)
    {
        // Given: un identificador invalido para el usuario.

        // When: se intenta preparar la suscripcion con ese id invalido.
        Result<SubscriptionContext> result = await _paymentService.PrepareSubscriptionAsync(invalidUserId!, CancellationToken.None);

        // Then: debe retornarse error por id de usuario invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    public async Task ShouldReturnUserNotFoundWhenUserDoesNotExist()
    {
        // Given: un identificador valido que no corresponde a ningun usuario.
        string nonExistentUserId = Guid.NewGuid().ToString();

        // When: se intenta preparar la suscripcion para ese usuario inexistente.
        Result<SubscriptionContext> result = await _paymentService.PrepareSubscriptionAsync(nonExistentUserId, CancellationToken.None);

        // Then: debe devolverse el error de usuario no encontrado.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
}
