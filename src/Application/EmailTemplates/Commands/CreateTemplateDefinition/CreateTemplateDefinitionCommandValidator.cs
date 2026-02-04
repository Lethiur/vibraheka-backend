using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;

public class CreateTemplateDefinitionCommandValidator : AbstractValidator<CreateTemplateDefinitionCommand>
{
    public CreateTemplateDefinitionCommandValidator()
    {
        RuleFor(x => x.TempateName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(EmailTemplateErrors.InvalidTemplateName)
            .NotNull()
            .WithMessage(EmailTemplateErrors.InvalidTemplateName)
            .MinimumLength(3)
            .WithMessage(EmailTemplateErrors.InvalidTemplateName);
    }
}
