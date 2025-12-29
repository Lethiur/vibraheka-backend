using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

public class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    AuthenticateUserCommandValidator()
    {
        RuleFor(x => x.email)
            .Cascade(CascadeMode.Stop).NotEmpty().WithMessage(UserException.InvalidEmail).NotNull()
            .WithMessage(UserException.InvalidEmail).ValidEmail()
            .WithMessage(UserException.InvalidEmail);
        RuleFor(x => x.password).Cascade(CascadeMode.Stop).MinimumLength(6).WithMessage(UserException.InvalidPassword)
            .NotNull().WithMessage(UserException.InvalidPassword);
    }
}
