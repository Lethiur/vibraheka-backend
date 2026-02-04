using VibraHeka.Application.Common.Extensions.Validation;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

public class CreateEmailTemplateCommandValidator : AbstractValidator<CreateEmailTemplateCommand>
{
    public CreateEmailTemplateCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.TemplateName).NotEmpty().WithMessage(EmailTemplateErrors.InvalidTemplateName).NotNull()
            .WithMessage(EmailTemplateErrors.InvalidTemplateName).MinimumLength(3)
            .WithMessage(EmailTemplateErrors.InvalidTemplateName);
        RuleFor(x => x.FileStream).NotNull().WithMessage(EmailTemplateErrors.InvalidTemplateContent).NotEmpty()
            .WithMessage(EmailTemplateErrors.InvalidTemplateContent);
    }
}
