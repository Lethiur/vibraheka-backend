using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions;

namespace VibraHeka.Application.Users.Commands;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().WithMessage(UserException.InvalidEmail).NotNull().WithMessage(UserException.InvalidEmail).ValidEmail()
            .WithMessage(UserException.InvalidEmail);
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).MinimumLength(6).WithMessage(UserException.InvalidPassword).NotNull().WithMessage(UserException.InvalidPassword);
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
