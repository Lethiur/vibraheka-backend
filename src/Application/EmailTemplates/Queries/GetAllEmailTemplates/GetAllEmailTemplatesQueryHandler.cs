using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;

/// <summary>
/// Handles the retrieval of all email templates within the system.
/// </summary>
/// <remarks>
/// This query handler processes a <see cref="GetAllEmailTemplatesQuery"/> to retrieve
/// a collection of email templates. It ensures that the current user has the authorization
/// to access this resource by validating the user's role as an administrator.
/// </remarks>
/// <param name="currentUserService">
/// Service to retrieve details about the currently authenticated user, including the user's ID.
/// </param>
/// <param name="privilegeService">
/// Service used to check if the current user has the appropriate roles or permissions to access resources.
/// </param>
/// <param name="emailTemplateService">
/// Service to interact with email templates, including operations to fetch all templates from the system.
/// </param>
public class GetAllEmailTemplatesQueryHandler(
    IEmailTemplatesService emailTemplateService) : IRequestHandler<GetAllEmailTemplatesQuery, Result<IEnumerable<EmailEntity>>>
{
    /// <summary>
    /// Handles the request to retrieve all email templates.
    /// </summary>
    /// <param name="request">The request object containing data required for retrieving email templates.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// wrapping an enumerable collection of <see cref="EmailEntity"/> objects if successful, or an error result otherwise.
    /// </returns>
    public Task<Result<IEnumerable<EmailEntity>>> Handle(GetAllEmailTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        return emailTemplateService.GetAllTemplates(cancellationToken);
    }
}
