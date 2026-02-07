using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Users.Commands.AdminCreateTherapist;

/// <summary>
/// Validator for the <c>CreateTherapistCommand</c> class.
/// Ensures that all required properties of the command are valid according to the specified rules.
/// </summary>
/// <remarks>
/// The validation logic includes the following rules:
/// - The <c>Email</c> property must be a valid and non-empty email address.
/// - The <c>Name</c> property must not be empty and will return a specific error message if validation fails.
/// </remarks>
public class CreateTherapistCommandValidator : AbstractValidator<CreateTherapistCommand>
{
    public CreateTherapistCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.TherapistData)
            .NotNull()
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("TherapistData is required.");
        
        // Email
        RuleFor(x => x.TherapistData.Email)
            .NotEmpty()
            .WithErrorCode(UserErrors.InvalidEmail)
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithErrorCode(UserErrors.InvalidEmail)
            .WithMessage("Email format is invalid.")
            .MaximumLength(320)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("Email is too long.");

        // Names (optional but validated if present)
        RuleFor(x => x.TherapistData.FirstName)
            .Must(BeEmptyOrNonWhitespace)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("FirstName cannot be whitespace.")
            .MaximumLength(100)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("FirstName is too long.");

        RuleFor(x => x.TherapistData.MiddleName)
            .Must(BeEmptyOrNonWhitespace)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("MiddleName cannot be whitespace.")
            .MaximumLength(100)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("MiddleName is too long.");

        RuleFor(x => x.TherapistData.LastName)
            .Must(BeEmptyOrNonWhitespace)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("LastName cannot be whitespace.")
            .MaximumLength(100)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("LastName is too long.");

        // Bio
        RuleFor(x => x.TherapistData.Bio)
            .MaximumLength(1000)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("Bio is too long.");

        // Profile picture URL (optional)
        RuleFor(x => x.TherapistData.ProfilePictureUrl)
            .MaximumLength(2048)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("ProfilePictureUrl is too long.")
            .Must(BeEmptyOrAbsoluteHttpUrl)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("ProfilePictureUrl must be empty or a valid absolute http/https URL.");

        // Phone number (optional)
        RuleFor(x => x.TherapistData.PhoneNumber)
            .MaximumLength(30)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("PhoneNumber is too long.")
            .Matches(@"^\+?[0-9\s\-\(\)]*$")
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("PhoneNumber contains invalid characters.");
    }
    
    private static bool BeValidGuid(string id) =>
        Guid.TryParse(id, out _);

    private static bool BeEmptyOrNonWhitespace(string value) =>
        string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value);

    private static bool BeEmptyOrAbsoluteHttpUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
