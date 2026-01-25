using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;

public class UpdateTemplateContentCommandValidator : AbstractValidator<UpdateTemplateContentCommand>
{
    public UpdateTemplateContentCommandValidator()
    {
        RuleFor(command => command.TemplateID).ValidTemplateID();
        RuleFor(command => command.TemplateStream).ValidJsonStream();
    }
        
}
