using System.Text.RegularExpressions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public partial class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(UserException.InvalidEmail)
            .NotNull()
            .WithMessage(UserException.InvalidEmail)
            .ValidEmail()
            .WithMessage(UserException.InvalidEmail);

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(UserException.InvalidVerificationCode)
            .NotNull()
            .WithMessage(UserException.InvalidVerificationCode)
            .MinimumLength(6)
            .WithMessage(UserException.InvalidVerificationCode);
    }
    
}
