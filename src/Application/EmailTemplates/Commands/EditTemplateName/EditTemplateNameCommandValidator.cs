using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;

public class EditTemplateNameCommandValidator : AbstractValidator<EditTemplateNameCommand>
{
    public EditTemplateNameCommandValidator()
    {
        RuleFor(x => x.TemplateID).Must(guid => guid != Guid.Empty).WithMessage(EmailTemplateErrors.InvalidTempalteID);
        
        RuleFor(x => x.NewTemplateName).NotEmpty().WithMessage(EmailTemplateErrors.InvalidTemplateName)
            .NotNull().WithMessage(EmailTemplateErrors.InvalidTemplateName)
            .MinimumLength(3).WithMessage(EmailTemplateErrors.InvalidTemplateName);
    }
}
