using Amazon.DynamoDBv2;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class GetSubscriptionDetailsUrlAsyncTest : TestBase
{
    private PaymentService _paymentService;
    private IPaymentRepository _paymentRepository;
    private IUserRepository _userRepository;
    private IAmazonDynamoDB _dynamoDbClient;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbClient = CreateDynamoDBClient();
        _userRepository = new UserRepository(CreateDynamoDBContext(), _dynamoDbClient, _configuration, CreateTestLogger<UserRepository>());
        _paymentRepository = new PaymentsRepository(
            _stripeConfig,
            _configuration,
            CreateSystemsManagementClient(),
            CreateTestLogger<PaymentsRepository>());
        _paymentService = new PaymentService(_paymentRepository, _userRepository);
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbClient?.Dispose();
    }

    [Test]
    public async Task ShouldReturnBillingPortalUrlWhenUserExistsWithCustomerId()
    {
        // Given: un usuario existente con customer id valido en Stripe.
        UserEntity userEntity = CreateValidUser();
        Result<string> registerCustomerResult = await _paymentRepository.RegisterCustomerAsync(userEntity, CancellationToken.None);
        userEntity.CustomerID = registerCustomerResult.Value;
        await _userRepository.AddAsync(userEntity);

        // When: se solicita la URL de detalles de suscripcion.
        Result<string> result = await _paymentService.GetSubscriptionDetailsUrlAsync(userEntity.Id, CancellationToken.None);

        // Then: debe devolverse una URL valida del portal de facturacion.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Value.StartsWith("https://billing.stripe.com/"), Is.True);
    }

    [Test]
    public async Task ShouldReturnUserNotFoundWhenUserDoesNotExist()
    {
        // Given: un id valido que no existe en la base de datos.
        string nonExistentUserId = Guid.NewGuid().ToString();

        // When: se solicita la URL para un usuario inexistente.
        Result<string> result = await _paymentService.GetSubscriptionDetailsUrlAsync(nonExistentUserId, CancellationToken.None);

        // Then: debe mapear al error de usuario no encontrado.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Test]
    public async Task ShouldReturnInvalidUserIdWhenUserIdIsInvalid(string? invalidUserId)
    {
        // Given: un id invalido (null, vacio o solo espacios).

        // When: se consulta la URL con ese id invalido.
        Result<string> result = await _paymentService.GetSubscriptionDetailsUrlAsync(invalidUserId!, CancellationToken.None);

        // Then: debe retornar error de id de usuario invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    public async Task ShouldReturnStripeErrorWhenUserHasInvalidCustomerId()
    {
        // Given: un usuario existente con customer id invalido para Stripe.
        UserEntity userEntity = CreateValidUser();
        userEntity.CustomerID = "cus_invalid_for_integration_test";
        await _userRepository.AddAsync(userEntity);

        // When: se solicita la URL de billing portal para ese usuario.
        Result<string> result = await _paymentService.GetSubscriptionDetailsUrlAsync(userEntity.Id, CancellationToken.None);

        // Then: debe devolverse el error tecnico de Stripe.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureSubscriptionErrors.StripeError));
    }
}
