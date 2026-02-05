using System;
using FluentValidation;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Application.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.NewUserData)
            .NotNull()
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("NewUserData is required.");
        
        // Id
        RuleFor(x => x.NewUserData.Id)
            .NotEmpty()
            .WithErrorCode(UserErrors.InvalidUserID)
            .WithMessage("User Id is required.")
            .Must(BeValidGuid)
            .WithErrorCode(UserErrors.InvalidUserID)
            .WithMessage("User Id must be a valid GUID.");

        // Email
        RuleFor(x => x.NewUserData.Email)
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
        RuleFor(x => x.NewUserData.FirstName)
            .Must(BeEmptyOrNonWhitespace)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("FirstName cannot be whitespace.")
            .MaximumLength(100)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("FirstName is too long.");

        RuleFor(x => x.NewUserData.MiddleName)
            .Must(BeEmptyOrNonWhitespace)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("MiddleName cannot be whitespace.")
            .MaximumLength(100)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("MiddleName is too long.");

        RuleFor(x => x.NewUserData.LastName)
            .Must(BeEmptyOrNonWhitespace)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("LastName cannot be whitespace.")
            .MaximumLength(100)
            .WithErrorCode(UserErrors.InvalidFullName)
            .WithMessage("LastName is too long.");

        // Bio
        RuleFor(x => x.NewUserData.Bio)
            .MaximumLength(1000)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("Bio is too long.");

        // Profile picture URL (optional)
        RuleFor(x => x.NewUserData.ProfilePictureUrl)
            .MaximumLength(2048)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("ProfilePictureUrl is too long.")
            .Must(BeEmptyOrAbsoluteHttpUrl)
            .WithErrorCode(UserErrors.InvalidForm)
            .WithMessage("ProfilePictureUrl must be empty or a valid absolute http/https URL.");

        // Phone number (optional)
        RuleFor(x => x.NewUserData.PhoneNumber)
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
