using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;

/// <summary>
/// Validator for <see cref="ChangeAuthenticatedPasswordCommand"/>.
/// </summary>
public class ChangeAuthenticatedPasswordCommandValidator : AbstractValidator<ChangeAuthenticatedPasswordCommand>
{
    /// <summary>
    /// Initializes validation rules for authenticated password change requests.
    /// </summary>
    public ChangeAuthenticatedPasswordCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidPassword)
            .NotNull()
            .WithMessage(UserErrors.InvalidPassword);

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
