using VibraHeka.Application.Common.Extensions.Validation;

namespace VibraHeka.Application.EmailTemplates.Queries.GetTemplateContent;

public class GetTemplateQueryValidator : AbstractValidator<GetEmailTemplateContentQuery>
{
    public GetTemplateQueryValidator()
    {
        RuleFor(x => x.TemplateID).ValidTemplateID();
    }
}
