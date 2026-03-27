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
    /// Updates the email template used when a password has been changed.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used after password changes.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{Unit}</c> indicating the success or failure of the operation.</returns>
    Task<Result<Unit>> ChangeRecoverPasswordEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the welcome email template sent to newly registered users.
    /// </summary>
    Task<Result<Unit>> ChangeUserWelcomeEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the subscription thank-you email template.
    /// </summary>
    Task<Result<Unit>> ChangeSubscriptionThankYouEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the email template used for notifying users about subscription cancellations.
    /// Ensures the provided email template is valid and updates the existing template in the repository.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used for subscription cancellation notifications.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{Unit}</c> representing the outcome of the operation, including any error details if applicable.</returns>
    Task<Result<Unit>> ChangeSubscriptionCancelledEmailTemplateAsync(string emailTemplate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the email template used for notifying users when their subscription is reactivated.
    /// Ensures the provided email template is valid and updates it in the repository.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used for subscription reactivation notifications.</param>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{Unit}</c> indicating the success or failure of the operation, including potential error details.</returns>
    Task<Result<Unit>> ChangeSubscriptionReActivatedEmailTemplateAsync(string emailTemplate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the trial-ending-soon email template.
    /// </summary>
    Task<Result<Unit>> ChangeTrialEndingSoonEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the password-changed confirmation email template.
    /// </summary>
    Task<Result<Unit>> ChangePasswordChangedEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the email template used for verification.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{String}</c> containing the verification template or a domain error.</returns>
    Task<Result<string>> GetVerificationEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the email template used when a password has been changed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{String}</c> containing the password changed template or a domain error.</returns>
    Task<Result<string>> GetRecoverPasswordEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the welcome email template used for newly registered users.
    /// </summary>
    Task<Result<string>> GetUserWelcomeEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the subscription thank-you email template.
    /// </summary>
    Task<Result<string>> GetSubscriptionThankYouEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the email template used for subscription cancellation notifications.
    /// Ensures the template is fetched from the repository, if available.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{string}</c> containing the subscription cancellation email template
    /// or error details if the operation fails.</returns>
    Task<Result<string>> GetSubscriptionCancelledEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the email template used for notifying users about subscription reactivation.
    /// Ensures the template is fetched from the repository if it exists.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to stop the task.</param>
    /// <returns>A <c>Result{string}</c> containing the email template or error details in case of failure.</returns>
    Task<Result<string>> GetSubscriptionReActivatedEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the trial-ending-soon email template.
    /// </summary>
    Task<Result<string>> GetTrialEndingSoonEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the password-changed confirmation email template.
    /// </summary>
    Task<Result<string>> GetPasswordChangedEmailTemplateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all templates available for specific actions from the settings service.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <c>Result<IEnumerable />
    ///         <TemplateForActionEntity>></c> containing a collection of templates for actions
    /// if retrieval is successful, or an error result if the operation fails.</returns>
    Result<IEnumerable<TemplateForActionEntity>> GetAllTemplatesForActions();
}
