using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Provides functionality to manage email template storage operations.
/// </summary>
public class EmailTemplateStorageService(IEmailTemplateStorageRepository repository) : IEmailTemplateStorageService
{
    /// <summary>
    /// Represents the repository used for managing email templates in the storage system.
    /// Provides methods for retrieving, saving, deleting, and generating access authorization
    /// strings for email templates.
    /// </summary>
    private readonly IEmailTemplateStorageRepository _repository = repository;

    /// <summary>
    /// Saves an email template to the storage repository.
    /// </summary>
    /// <param name="templateID">The unique identifier for the email template.</param>
    /// <param name="templateStream">The stream containing the email template content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the template ID if the operation is successful, or an error if it fails.</returns>
    public Task<Result<string>> SaveTemplate(string templateID, Stream templateStream,
        CancellationToken cancellationToken)
    {
        return _repository.SaveTemplate(templateID, templateStream, cancellationToken);
    }
}
