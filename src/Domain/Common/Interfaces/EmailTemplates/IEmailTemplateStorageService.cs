using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.EmailTemplates;

/// <summary>
/// Represents a service interface for managing and accessing email templates in storage.
/// </summary>
public interface IEmailTemplateStorageService
{
    /// <summary>
    /// Saves an email template to the storage system.
    /// </summary>
    /// <param name="templateID">The unique identifier for the email template.</param>
    /// <param name="templateStream">The stream containing the email template content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing a Result indicating the success or failure of the save operation.</returns>
    Task<Result<string>> SaveTemplate(string templateID, Stream templateStream, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an attachment to an email template in the storage system.
    /// </summary>
    /// <param name="templateID">The unique identifier for the email template.</param>
    /// <param name="attachment">The stream containing the content of the attachment.</param>
    /// <param name="attachmentName">The name of the attachment to be added.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing a Result with the URL of the uploaded attachment or an error if the operation fails.</returns>
    Task<Result<string>> AddAttachment(string templateID, Stream attachment, string attachmentName,
        CancellationToken cancellationToken);
}
