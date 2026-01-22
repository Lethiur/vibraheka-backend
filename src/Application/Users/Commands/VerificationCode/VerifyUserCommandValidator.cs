using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public partial class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .ValidEmail();

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidVerificationCode)
            .NotNull()
            .Matches(@"^\d+$")
            .WithMessage(UserErrors.InvalidVerificationCode)
            .MinimumLength(6)
            .WithMessage(UserErrors.InvalidVerificationCode);
    }
    
}
