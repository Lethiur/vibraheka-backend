using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.AdminCreateTherapist;

/// <summary>
/// Validator for the <c>CreateTherapistCommand</c> class.
/// Ensures that all required properties are valid according to business rules.
/// </summary>
public class CreateTherapistCommandValidator : AbstractValidator<CreateTherapistCommand>
{
    public CreateTherapistCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(320)
            .WithMessage(UserErrors.EmailTooLong)
            .ValidEmail();

        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidFullName);

        RuleFor(x => x.MiddleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidFullName);

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidFullName);

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .WithMessage(UserErrors.InvalidForm);

        RuleFor(x => x.ProfilePictureUrl)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(2048)
            .WithMessage(UserErrors.InvalidForm)
            .ValidURL()
            .WithMessage(UserErrors.InvalidForm)
            .When(x => !string.IsNullOrWhiteSpace(x.ProfilePictureUrl) && !string.IsNullOrEmpty(x.ProfilePictureUrl));

        RuleFor(x => x.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(30)
            .WithMessage(UserErrors.InvalidForm)
            .Matches(@"^\+?[0-9\s\-\(\)]*$")
            .WithMessage(UserErrors.InvalidForm)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber) && !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.TimezoneID)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidForm)
            .Must(tz => !string.IsNullOrWhiteSpace(tz))
            .WithMessage(UserErrors.InvalidForm)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidForm);
    }
}
