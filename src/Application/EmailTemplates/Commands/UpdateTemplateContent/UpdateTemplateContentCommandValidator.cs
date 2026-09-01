using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;

public class UpdateTemplateContentCommandValidator : AbstractValidator<UpdateTemplateContentCommand>
{
    public UpdateTemplateContentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.TemplateID).Must(guid => guid != Guid.Empty).WithMessage(EmailTemplateErrors.InvalidTempalteID);
        RuleFor(command => command.TemplateStream).NotNull().WithMessage(EmailTemplateErrors.InvalidTemplateContent)
            .Must((stream) => stream.CanSeek && stream.Length > 0)
            .WithMessage(EmailTemplateErrors.InvalidTemplateContent);
    }
}
