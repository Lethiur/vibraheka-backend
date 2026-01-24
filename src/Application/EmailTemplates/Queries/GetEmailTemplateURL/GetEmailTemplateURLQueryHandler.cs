using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;

/// <summary>
/// Handles the retrieval of an email template URL based on the specified template ID.
/// </summary>
/// <remarks>
/// This class processes a query to get the URL of an email template by verifying user credentials
/// and privileges, ensuring that the user has the appropriate access rights to retrieve the template.
/// It depends on services for user identification, privilege checking, and email template storage.
/// </remarks>
/// <param name="emailTemplateStorageService">
/// Offers functionality to interact with the storage mechanism for email templates.
/// </param>
public class GetEmailTemplateURLQueryHandler(
    IEmailTemplateStorageService emailTemplateStorageService) : IRequestHandler<GetEmailTemplateURLQuery, Result<string>> 
{
    public Task<Result<string>> Handle(GetEmailTemplateURLQuery request, CancellationToken cancellationToken)
    {
        return  emailTemplateStorageService.GetTemplateUrlAsync(request.TemplateID, cancellationToken);
    }
}
