using VibraHeka.Application.Common.Extensions.Validation;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.AddAttachment;

public class AddAttachmentCommandValidator : AbstractValidator<AddAttachmentCommand>
{
    public AddAttachmentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.AttachmentName)
            .NotEmpty()
            .WithMessage(EmailTemplateErrors.InvalidAttachmentName)
            .MinimumLength(3)
            .WithMessage(EmailTemplateErrors.InvalidAttachmentName);

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage(EmailTemplateErrors.InvalidAttachmentContent)
            .NotEmpty()
            .WithMessage(EmailTemplateErrors.InvalidAttachmentContent)
            .ValidImageOrVideoStream()
            .WithMessage(EmailTemplateErrors.InvalidAttachmentContent);
        
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(EmailTemplateErrors.InvalidTemplateId);
    }
}
