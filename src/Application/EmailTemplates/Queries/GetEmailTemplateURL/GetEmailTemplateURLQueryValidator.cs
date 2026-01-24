using VibraHeka.Application.Common.Extensions.Validation;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;

public class GetEmailTemplateURLQueryValidator : AbstractValidator<GetEmailTemplateURLQuery>
{
    public GetEmailTemplateURLQueryValidator()
    {
        RuleFor(query => query.TemplateID).ValidTemplateID();
    }
}
