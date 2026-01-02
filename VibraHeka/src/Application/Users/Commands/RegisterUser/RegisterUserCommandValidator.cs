using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).ValidEmail();
        
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).ValidPassword();
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserException.InvalidFullName)
            .Must(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;
            
                var trimmed = value.Trim();
                return trimmed.Length >= 3;
            })
            .WithMessage(UserException.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserException.InvalidFullName);
    }
}
