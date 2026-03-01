using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.StartPasswordRecovery;

/// <summary>
/// Validator for <see cref="StartPasswordRecoveryCommand"/>.
/// </summary>
public class StartPasswordRecoveryCommandValidator : AbstractValidator<StartPasswordRecoveryCommand>
{
    /// <summary>
    /// Initializes validation rules for password recovery start requests.
    /// </summary>
    public StartPasswordRecoveryCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Email).ValidEmail();
    }
}
