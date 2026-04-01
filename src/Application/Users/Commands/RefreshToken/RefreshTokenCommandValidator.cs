using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.RefreshToken).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(UserErrors.InvalidForm)
            .MinimumLength(20)
            .WithMessage(UserErrors.InvalidForm)
            .MaximumLength(4096)
            .WithMessage(UserErrors.InvalidForm)
            .Must(t => !t.Any(char.IsWhiteSpace)).WithMessage(UserErrors.InvalidForm)
            // Cognito refresh_token no es JWT; esto valida "forma", no "validez" real.
            .Matches(@"^[A-Za-z0-9+/_=.\-]+$").WithMessage(UserErrors.InvalidForm);
        ;
    }
}
