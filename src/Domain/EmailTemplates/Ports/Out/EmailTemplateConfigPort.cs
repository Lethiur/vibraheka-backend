using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Domain.EmailTemplates.Ports.Out;

public interface EmailTemplateConfigPort
{
    /// <summary>
    /// Updates the email template used for verification purposes.
    /// Validates that the provided email template is not null or whitespace,
    /// and updates the template in the repository if valid.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used for verification.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the task</param>
    /// <returns>A <c>Result{Unit}</c> indicating the success or failure of the operation,
    /// including potential error details.</returns>
    Task<Result<Unit>> ChangeEmailTemplateKeyForAction(string emailTemplateKey, string emailTemplateID, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the email template ID associated with the given email template key.
    /// Validates the provided key and retrieves the corresponding template from the repository
    /// if it exists.
    /// </summary>
    /// <param name="emailTemplateKey">The key representing the email template to be retrieved.</param>
    /// <returns>A <c>Result{string}</c> containing the email template ID if found,
    /// or an error detail describing the issue.</returns>
    Task<Result<string>> GetEmailTemplateKeyForAction(string emailTemplateKey);
}
