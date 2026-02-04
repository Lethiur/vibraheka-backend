using VibraHeka.Application.Common.Extensions.Validation;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;

public class UpdateTemplateContentCommandValidator : AbstractValidator<UpdateTemplateContentCommand>
{
    public UpdateTemplateContentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(command => command.TemplateID).ValidTemplateID();
        RuleFor(command => command.TemplateStream).NotNull().WithMessage(EmailTemplateErrors.InvalidTemplateContent)
            .Must((stream) => stream.CanSeek && stream.Length > 0)
            .WithMessage(EmailTemplateErrors.InvalidTemplateContent);
    }
}
