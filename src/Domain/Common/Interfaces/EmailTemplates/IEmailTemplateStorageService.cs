using CSharpFunctionalExtensions;
using MediatR;

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
}
