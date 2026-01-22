using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Domain.Common.Interfaces.EmailTemplates;

/// <summary>
/// Defines the operations for managing email templates in the storage repository.
/// Provides functionality to retrieve, save, delete, and authorize access to email templates.
/// </summary>
public interface IEmailTemplateStorageRepository
{
    /// <summary>
    /// Retrieves an email template from the storage repository using the specified template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation,
    /// containing a <c>Result</c> object with a <c>Stream</c> of the email template on success, or an error on failure.</returns>
    Task<Result<Stream>> GetTemplate(string templateID, CancellationToken cancellationToken);

    /// <summary>
    /// Saves an email template to the storage repository using the specified template ID and content stream.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to save.</param>
    /// <param name="templateStream">The <c>Stream</c> containing the content of the email template to be saved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation,
    /// containing a <c>Result</c> object indicating success or failure of the save operation.</returns>
    Task<Result<Unit>> SaveTemplate(string templateID, Stream templateStream, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an authorization string that can be used to access the specified email template for reading.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template for which the authorization string is requested.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation,
    /// containing a <c>Result</c> object with the authorization string on success, or an error on failure.</returns>
    Task<Result<string>> GetAuthorizationStringForTemplateRead(string templateID, CancellationToken cancellationToken);

    /// <summary>
    /// Generates an authorization string for write access to a specific email template in the storage repository.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template for which the authorization string is requested.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation,
    /// containing a <c>Result</c> object with the authorization string on success, or an error on failure.</returns>
    Task<Result<string>> GetAuthorizationStringForTemplateWrite(string templateID, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an email template from the storage repository using the specified template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation,
    /// containing a <c>Result</c> object indicating success or failure of the delete operation.</returns>
    Task<Result<Unit>> DeleteTemplate(string templateID, CancellationToken cancellationToken);
}
