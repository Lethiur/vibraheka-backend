using VibraHeka.Application.Common.Extensions.Validation;
using static VibraHeka.Domain.Exceptions.EmailTemplateErrors;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

public class CreateEmailTemplateCommandValidator : AbstractValidator<CreateEmailTemplateCommand>
{
    public CreateEmailTemplateCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.TemplateName).NotEmpty().WithMessage(InvalidTemplateName).NotNull()
            .WithMessage(InvalidTemplateName).MinimumLength(3)
            .WithMessage(InvalidTemplateName);
        RuleFor(x => x.FileStream).NotNull().WithMessage(InvalidTemplateContent).NotEmpty()
            .WithMessage(InvalidTemplateContent).Must(stream => stream.CanSeek && stream.Length > 0).WithMessage(InvalidTemplateContent);
    }
}
