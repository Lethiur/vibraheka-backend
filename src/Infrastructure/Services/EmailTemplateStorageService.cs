using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

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


    /// <summary>
    /// Adds an attachment to the storage repository associated with a specific email template.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to which the attachment will be associated.</param>
    /// <param name="attachment">A stream containing the content of the attachment to be added.</param>
    /// <param name="attachmentName">The name of the attachment file to be stored.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing a result with the identifier of the saved attachment on success, or an error in case of failure.</returns>
    public Task<Result<string>> AddAttachment(string templateID, Stream attachment, string attachmentName,
        CancellationToken cancellationToken)
    {
        return _repository.SaveAttachment(templateID, attachment, attachmentName, cancellationToken);
    }

    /// <summary>
    /// Retrieves the URL of a stored email template.
    /// </summary>
    /// <param name="templateID">The unique identifier for the email template.</param>
    /// <param name="cancellationToken">The token to cancel the task preemptively</param>
    /// <returns>A result containing the URL of the email template if the operation is successful, or an error if it fails.</returns>
    public Task<Result<string>> GetTemplateUrlAsync(string templateID, CancellationToken cancellationToken)
    {
        return Result.Of(templateID)
            .Ensure(async tpl => await _repository.TemplateExistsAsync(tpl, cancellationToken))
            .BindTry(tpl =>_repository.GetTemplateUrlAsync(tpl) );
    }

    public Task<Result<string>> GetTemplateContent(string templateID, CancellationToken cancellationToken)
    {
        return _repository.GetTemplateContent(templateID, cancellationToken);
    }
}
