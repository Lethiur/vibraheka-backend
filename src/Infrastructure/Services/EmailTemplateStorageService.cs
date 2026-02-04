using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Exceptions;
using static VibraHeka.Domain.Exceptions.EmailTemplateErrors;

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
            .Ensure(tpl => !string.IsNullOrWhiteSpace(tpl) && !string.IsNullOrEmpty(tpl), InvalidTempalteID)
            .Check(tpl => CheckTemplateExists(tpl, cancellationToken))
            .BindTry(tpl =>_repository.GetTemplateUrlAsync(tpl) );
    }

    /// <summary>
    /// Retrieves the content of an email template from the storage repository.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the email template content as a string if successful, or an error if the operation fails.</returns>
    public Task<Result<string>> GetTemplateContent(string templateID, CancellationToken cancellationToken)
    {
        return Maybe.From(templateID).ToResult(InvalidTempalteID)
            .BindTry(tplID => CheckTemplateExists(tplID, cancellationToken))
            .BindTry(_ => _repository.GetTemplateContent(templateID, cancellationToken));
    }

    /// <summary>
    /// Checks whether a template exists in the storage repository.
    /// </summary>
    /// <param name="templateID">The unique identifier of the template to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A boolean value indicating whether the template exists.</returns>
    public Task<Result<Unit>> CheckTemplateExists(string templateID, CancellationToken cancellationToken)
    {
        return Maybe.From(templateID)
            .ToResult(InvalidTempalteID)
            .Ensure(a => !string.IsNullOrWhiteSpace(a), InvalidTempalteID)
            .BindTry(_ => _repository.TemplateExistsAsync(templateID, cancellationToken))
            .Ensure(exist => exist, TemplateNotFound)
            .Map(_ => Unit.Value);
    }
}
