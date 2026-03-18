using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class GetUserByIDTest : TestBase
{
    private PaymentService _paymentService;
    private IUserRepository _userRepository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _userRepository = new UserRepository(CreateDynamoDBContext(), _configuration);
        _paymentService = new PaymentService(
            new PaymentsRepository(_stripeConfig, CreateTestLogger<PaymentsRepository>()),
            _userRepository);
    }

    [Test]
    public async Task ShouldReturnUserWhenIdIsValidAndUserExists()
    {
        // Given: un usuario valido almacenado en la base de datos.
        UserEntity userEntity = CreateValidUser();
        await _userRepository.AddAsync(userEntity);

        // When: se consulta el usuario por su id valido.
        Result<UserEntity> result = await _paymentService.GetUserByID(userEntity.Id, CancellationToken.None);

        // Then: debe devolverse el usuario correctamente.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(userEntity.Id));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [Test]
    public async Task ShouldReturnInvalidUserIdWhenInputIsNullOrWhitespace(string? invalidUserId)
    {
        // Given: un id invalido (null, vacio o solo espacios).

        // When: se intenta obtener el usuario con ese id invalido.
        Result<UserEntity> result = await _paymentService.GetUserByID(invalidUserId!, CancellationToken.None);

        // Then: debe retornar error de id invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));
    }

    [Test]
    public async Task ShouldReturnUserNotFoundWhenUserDoesNotExist()
    {
        // Given: un id valido que no existe en persistencia.
        string nonExistentUserId = Guid.NewGuid().ToString();

        // When: se consulta el usuario inexistente.
        Result<UserEntity> result = await _paymentService.GetUserByID(nonExistentUserId, CancellationToken.None);

        // Then: debe mapear al error de usuario no encontrado.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
}
