using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;

/// <summary>
/// Command used by an authenticated user to change their own password.
/// </summary>
/// <param name="CurrentPassword">Current user password.</param>
/// <param name="NewPassword">New password to set.</param>
/// <param name="NewPasswordConfirmation">Confirmation for the new password.</param>
public record ChangeAuthenticatedPasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string NewPasswordConfirmation) : IRequest<Result<Unit>>;
