using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.AuthenticateUsers;

public class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop).ValidEmail();
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).ValidPassword();
    }
}
