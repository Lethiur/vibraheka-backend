using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Extensions.Validation;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

public class CreateEmailTemplateCommandValidator : AbstractValidator<CreateEmailTemplateCommand>
{
    public CreateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.templateName).NotEmpty().NotNull().MinimumLength(3).WithMessage(EmailTemplateErrors.InvalidTemplateName);
        RuleFor(x => x.fileStream).NotNull().NotEmpty().ValidJsonStream().WithMessage(EmailTemplateErrors.InvalidTemplateContent);
    }
}
