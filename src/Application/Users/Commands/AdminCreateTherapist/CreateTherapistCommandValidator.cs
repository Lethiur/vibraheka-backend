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

        RuleFor(x => x.TherapistData)
            .NotNull()
            .WithMessage(UserErrors.InvalidForm);

        RuleFor(x => x.TherapistData.Email)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(320)
            .WithMessage(UserErrors.EmailTooLong)
            .ValidEmail();

        RuleFor(x => x.TherapistData.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidFullName);

        RuleFor(x => x.TherapistData.MiddleName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidFullName);

        RuleFor(x => x.TherapistData.LastName)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidFullName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage(UserErrors.InvalidFullName)
            .MinimumLength(3)
            .WithMessage(UserErrors.InvalidFullName)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidFullName);

        RuleFor(x => x.TherapistData.Bio)
            .MaximumLength(1000)
            .WithMessage(UserErrors.InvalidForm);

        RuleFor(x => x.TherapistData.ProfilePictureUrl)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(2048)
            .WithMessage(UserErrors.InvalidForm)
            .ValidURL()
            .WithMessage(UserErrors.InvalidForm)
            .When(x => !string.IsNullOrWhiteSpace(x.TherapistData.ProfilePictureUrl) && !string.IsNullOrEmpty(x.TherapistData.ProfilePictureUrl));

        RuleFor(x => x.TherapistData.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(30)
            .WithMessage(UserErrors.InvalidForm)
            .Matches(@"^\+?[0-9\s\-\(\)]*$")
            .WithMessage(UserErrors.InvalidForm)
            .When(x => !string.IsNullOrWhiteSpace(x.TherapistData.PhoneNumber) && !string.IsNullOrEmpty(x.TherapistData.PhoneNumber));

        RuleFor(x => x.TherapistData.TimezoneID)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(UserErrors.InvalidForm)
            .Must(tz => !string.IsNullOrWhiteSpace(tz))
            .WithMessage(UserErrors.InvalidForm)
            .MaximumLength(100)
            .WithMessage(UserErrors.InvalidForm);
    }
}
