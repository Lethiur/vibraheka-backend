using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Common.Extensions.Validation;

/// <summary>
/// Provides an extension method for validating Template IDs in the context of FluentValidation rules.
/// This extension ensures that the Template ID is not null, not empty, and conforms to a valid GUID format.
/// </summary>
public static class TemplateIDValidationExtension
{
    public static IRuleBuilderOptions<T, string> ValidTemplateID<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(EmailTemplateErrors.InvalidTempalteID)
            .NotNull()
            .WithMessage(EmailTemplateErrors.InvalidTempalteID)
            .Must(templateID => Guid.TryParse(templateID, out _))
            .WithMessage(EmailTemplateErrors.InvalidTempalteID);
    }
}
