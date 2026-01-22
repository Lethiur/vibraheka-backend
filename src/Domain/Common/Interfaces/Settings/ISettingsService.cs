using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.Settings;

/// <summary>
/// Defines the contract for a service that manages application settings and
/// provides functionality related to email verification templates.
/// </summary>
public interface ISettingsService
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
    Task<Result<Unit>> ChangeEmailForVerificationAsync(string emailTemplate, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all templates available for specific actions from the settings service.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Result<IEnumerable />
    ///         <TemplateForActionEntity>></c> containing a collection of templates for actions
    /// if retrieval is successful, or an error result if the operation fails.</returns>
    Result<IEnumerable<TemplateForActionEntity>> GetAllTemplatesForActions();
}
