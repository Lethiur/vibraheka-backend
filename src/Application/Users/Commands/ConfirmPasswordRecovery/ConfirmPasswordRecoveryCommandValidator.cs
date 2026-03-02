using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;

/// <summary>
/// Validator for <see cref="ConfirmPasswordRecoveryCommand"/>.
/// </summary>
public class ConfirmPasswordRecoveryCommandValidator : AbstractValidator<ConfirmPasswordRecoveryCommand>
{
    /// <summary>
    /// Initializes validation rules for password recovery confirmation.
    /// </summary>
    public ConfirmPasswordRecoveryCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.EncryptedToken)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidPasswordResetToken)
            .NotNull()
            .WithMessage(UserErrors.InvalidPasswordResetToken);

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .ValidPassword();

        RuleFor(x => x.NewPasswordConfirmation)
            .Cascade(CascadeMode.Stop)
            .ValidPassword()
            .Equal(x => x.NewPassword)
            .WithMessage(UserErrors.InvalidPassword);
    }
}
