using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.ResendConfirmationCode;

public class ResendConfirmationCodeValidator : AbstractValidator<ResendConfirmationCodeCommand>
{
    public ResendConfirmationCodeValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop; 
        RuleFor(x => x.Email).ValidEmail();
    }
}
