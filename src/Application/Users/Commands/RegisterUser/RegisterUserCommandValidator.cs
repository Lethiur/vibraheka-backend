using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop; 
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).ValidEmail();
        
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).ValidPassword();
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;
            
                string trimmed = value.Trim();
                return trimmed.Length >= 3;
            })
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName);
    }
}
