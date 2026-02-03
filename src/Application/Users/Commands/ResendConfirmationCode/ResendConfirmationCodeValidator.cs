using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.ResendConfirmationCode;

public class ResendConfirmationCodeValidator : AbstractValidator<ResendConfirmationCodeCommand>
{
    public ResendConfirmationCodeValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
    }
}
