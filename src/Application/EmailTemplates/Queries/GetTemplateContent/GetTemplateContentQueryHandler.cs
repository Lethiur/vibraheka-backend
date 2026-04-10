using CSharpFunctionalExtensions;
using VibraHeka.Domain.EmailTemplates.Ports.Out;

namespace VibraHeka.Application.EmailTemplates.Queries.GetTemplateContent;

public class GetTemplateContentQueryHandler(
    EmailTemplatePort templatesService,
    EmailTemplateContentPort templateStorageService) : IRequestHandler<GetEmailTemplateContentQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetEmailTemplateContentQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(request.TemplateID)
            .Bind((templateID) => templatesService.GetTemplateByID(templateID, cancellationToken))
            .Bind(emailTemplate =>
                templateStorageService.GetTemplateContent(emailTemplate.TemplateID, cancellationToken));
    }
}
