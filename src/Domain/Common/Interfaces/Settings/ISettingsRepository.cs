using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Domain.Common.Interfaces.Settings;

/// <summary>
/// Represents a repository that manages settings related to the application.
/// Provides methods for updating and retrieving the verification email template.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Updates the verification email template in the settings repository.
    /// </summary>
    /// <param name="emailTemplate">
    /// The verification email template to be updated.
    /// </param>
    /// <param name="token">The cancellation token to listen for cancellations</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating
    /// the success or failure of the operation.
    /// </returns>
    Task<Result<Unit>> UpdateVerificationEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Retrieves the verification email template from the settings repository.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{String}"/>
    /// holding the current verification email template, or an error indicating the failure of the operation.
    /// </returns>
    Task<Result<string>> GetVerificationEmailTemplateAsync();
    
}
