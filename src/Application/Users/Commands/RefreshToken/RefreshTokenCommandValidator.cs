using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.RefreshToken).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(20)
            .MaximumLength(4096)
            .Must(t => !t.Any(char.IsWhiteSpace)).WithMessage("refreshToken must not contain whitespace.")
            // Cognito refresh_token no es JWT; esto valida "forma", no "validez" real.
            .Matches(@"^[A-Za-z0-9+/_=.\-]+$").WithMessage("refreshToken has invalid characters.");
        ;
    }
}
