using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Admin.Commands.CreateTherapist;

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
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).ValidEmail();
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop).NotEmpty().WithMessage(UserException.InvalidFullName);
    }
}
