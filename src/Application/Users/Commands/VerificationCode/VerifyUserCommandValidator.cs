using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public partial class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.EncryptedCode)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidPasswordResetToken)
            .NotNull()
            .WithMessage(UserErrors.InvalidPasswordResetToken);
    }

}
