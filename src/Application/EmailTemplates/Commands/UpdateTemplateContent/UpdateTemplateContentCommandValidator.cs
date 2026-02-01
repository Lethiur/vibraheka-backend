using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ganss.Xss;
using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;

public class UpdateTemplateContentCommandValidator : AbstractValidator<UpdateTemplateContentCommand>
{
    public UpdateTemplateContentCommandValidator()
    {
        RuleFor(command => command.TemplateID).ValidTemplateID();
        RuleFor(command => command.TemplateStream).NotNull().Must((stream) => stream.CanSeek && stream.Length > 0);
    }
        
}
