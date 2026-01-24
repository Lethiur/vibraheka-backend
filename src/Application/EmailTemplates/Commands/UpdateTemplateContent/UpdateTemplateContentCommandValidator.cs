using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplate;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateID).ValidTemplateID();
        RuleFor(command => command.TemplateStream).ValidJsonStream();
    }
        
}
