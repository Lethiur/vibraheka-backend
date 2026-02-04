using VibraHeka.Application.Common.Extensions.Validation;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;

public class EditTemplateNameCommandValidator : AbstractValidator<EditTemplateNameCommand>
{
    public EditTemplateNameCommandValidator()
    {
        RuleFor(x => x.TemplateID).ValidTemplateID();
        RuleFor(x => x.NewTemplateName).NotEmpty().WithMessage(EmailTemplateErrors.InvalidTemplateName)
            .NotNull().WithMessage(EmailTemplateErrors.InvalidTemplateName)
            .MinimumLength(3).WithMessage(EmailTemplateErrors.InvalidTemplateName);
    }
}
