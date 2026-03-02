using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;

/// <summary>
/// Command used to confirm password recovery with an encrypted token and new password pair.
/// </summary>
/// <param name="EncryptedToken">Encrypted token sent to the user by the recovery email flow.</param>
/// <param name="NewPassword">New password to set in Cognito.</param>
/// <param name="NewPasswordConfirmation">Password confirmation value.</param>
public record ConfirmPasswordRecoveryCommand(
    string EncryptedToken,
    string NewPassword,
    string NewPasswordConfirmation) : IRequest<Result<Unit>>;
