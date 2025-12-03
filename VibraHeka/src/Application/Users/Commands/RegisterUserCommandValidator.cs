using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Application.Users.Commands;

public class RegisterUserCommandValidator: AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(UserException.InvalidEmail);
        RuleFor(x => x.Password).MinimumLength(6).WithMessage(UserException.InvalidPassword);
        RuleFor(x => x.FullName).NotEmpty().WithMessage(UserException.InvalidFullName);
    }
}
