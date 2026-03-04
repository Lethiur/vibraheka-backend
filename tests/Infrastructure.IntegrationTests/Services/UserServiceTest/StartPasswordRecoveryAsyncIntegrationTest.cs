using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class StartPasswordRecoveryAsyncIntegrationTest : GenericCognitoServiceTest
{
    [Test]
    [DisplayName("Should start password recovery for confirmed user")]
    public async Task ShouldStartPasswordRecoveryForConfirmedUser()
    {
        // Given: un usuario registrado y confirmado.
        string email = GenerateUniqueEmail("test-start-recovery@");
        await RegisterUser(email);
        Result<VerificationCodeEntity> codeResult = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        Result<Unit> confirmResult = await _userService.ConfirmUserAsync(email, codeResult.Value.Code);
        Assert.That(confirmResult.IsSuccess, Is.True);

        // When: se inicia el flujo forgot-password.
        Result<Unit> result = await _userService.StartPasswordRecoveryAsync(email);

        // Then: la operacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    [DisplayName("Should return user not found when email does not exist")]
    public async Task ShouldReturnUserNotFoundWhenEmailDoesNotExist()
    {
        // Given: un email inexistente.
        string email = $"ghost-{Guid.NewGuid():N}@example.com";

        // When: se inicia recovery para usuario inexistente.
        Result<Unit> result = await _userService.StartPasswordRecoveryAsync(email);

        // Then: debe mapear a UserNotFound.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [DisplayName("Should return invalid form when email is invalid")]
    public async Task ShouldReturnInvalidFormWhenEmailIsInvalid(string? invalidEmail)
    {
        // Given: email invalido.

        // When: se inicia recovery con email invalido.
        Result<Unit> result = await _userService.StartPasswordRecoveryAsync(invalidEmail!);

        // Then: debe mapear a InvalidForm.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm));
    }
}
