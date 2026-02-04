using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;

public class ChangeTemplateForActionCommandValidator : AbstractValidator<ChangeTemplateForActionCommand>
{
    public ChangeTemplateForActionCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop; 
        RuleFor(x => x.TemplateID).ValidTemplateID();
    }
}
