using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class ConfirmPasswordRecoveryAsyncIntegrationTest : GenericCognitoServiceTest
{
    [Test]
    [DisplayName("Should return wrong verification code when recovery code is invalid")]
    public async Task ShouldReturnWrongVerificationCodeWhenRecoveryCodeIsInvalid()
    {
        // Given: un usuario confirmado con flujo recovery iniciado.
        string email = GenerateUniqueEmail("test-confirm-recovery-invalid-code@");
        const string initialPassword = "ValidPassword123!";
        const string newPassword = "RecoveredPassword123!";

        Result<string> registerResult = await _userService.RegisterUserAsync(email, initialPassword, "Recovery Invalid Code");
        Assert.That(registerResult.IsSuccess, Is.True);
        Result<VerificationCodeEntity> verificationCode = await WaitForVerificationCode(email, TimeSpan.FromSeconds(10));
        Result<Unit> confirmUserResult = await _userService.ConfirmUserAsync(email, verificationCode.Value.Code);
        Assert.That(confirmUserResult.IsSuccess, Is.True);
        Result<Unit> startRecoveryResult = await _userService.StartPasswordRecoveryAsync(email);
        Assert.That(startRecoveryResult.IsSuccess, Is.True);

        // When: se confirma con un codigo incorrecto.
        Result<Unit> confirmRecoveryResult = await _userService.ConfirmPasswordRecoveryAsync(
            email,
            "000000",
            newPassword,
            CancellationToken.None);

        // Then: debe mapear a WrongVerificationCode.
        Assert.That(confirmRecoveryResult.IsFailure, Is.True);
        Assert.That(confirmRecoveryResult.Error, Is.EqualTo(UserErrors.WrongVerificationCode));
    }

    [Test]
    [DisplayName("Should return user not found when recovery user does not exist")]
    public async Task ShouldReturnUserNotFoundWhenRecoveryUserDoesNotExist()
    {
        // Given: email inexistente.
        string email = $"ghost-{Guid.NewGuid():N}@example.com";

        // When: se intenta confirmar recovery para usuario inexistente.
        Result<Unit> confirmRecoveryResult = await _userService.ConfirmPasswordRecoveryAsync(
            email,
            "123456",
            "RecoveredPassword123!",
            CancellationToken.None);

        // Then: debe mapear a UserNotFound.
        Assert.That(confirmRecoveryResult.IsFailure, Is.True);
        Assert.That(confirmRecoveryResult.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should return invalid form when recovery payload is invalid")]
    public async Task ShouldReturnInvalidFormWhenRecoveryPayloadIsInvalid()
    {
        // Given: payload invalido.

        // When: se intenta confirmar recovery con datos invalidos.
        Result<Unit> confirmRecoveryResult = await _userService.ConfirmPasswordRecoveryAsync(
            "",
            "",
            "",
            CancellationToken.None);

        // Then: debe mapear a InvalidForm.
        Assert.That(confirmRecoveryResult.IsFailure, Is.True);
        Assert.That(confirmRecoveryResult.Error, Is.EqualTo(UserErrors.InvalidForm));
    }
}
