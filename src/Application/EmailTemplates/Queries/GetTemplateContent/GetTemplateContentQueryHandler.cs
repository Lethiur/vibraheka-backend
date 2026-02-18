using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Application.EmailTemplates.Queries.GetTemplateContent;

public class GetTemplateContentQueryHandler(IEmailTemplatesService templatesService, IEmailTemplateStorageService templateStorageService) : IRequestHandler<GetEmailTemplateContentQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetEmailTemplateContentQuery request, CancellationToken cancellationToken)
    {
        return Result.Success(request.TemplateID).Bind((templateID) => templatesService.GetTemplateByID(templateID, cancellationToken))
            .Bind(emailTemplate => templateStorageService.GetTemplateContent(emailTemplate.ID, cancellationToken));
    }
}
