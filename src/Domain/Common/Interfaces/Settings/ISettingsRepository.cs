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
    /// Updates the password changed email template in the settings repository.
    /// </summary>
    /// <param name="emailTemplate">
    /// The password changed email template to be updated.
    /// </param>
    /// <param name="token">The cancellation token to listen for cancellations.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating
    /// the success or failure of the operation.
    /// </returns>
    Task<Result<Unit>> UpdateRecoverPasswordEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the welcome email template for newly registered users.
    /// </summary>
    Task<Result<Unit>> UpdateUserWelcomeEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the subscription thank-you email template.
    /// </summary>
    Task<Result<Unit>> UpdateSubscriptionThankYouEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the subscription cancelled email template in the settings repository.
    /// </summary>
    /// <param name="emailTemplate">
    /// The subscription cancelled email template to be updated.
    /// </param>
    /// <param name="token">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating
    /// whether the operation succeeded or failed.
    /// </returns>
    Task<Result<Unit>> UpdateSubscriptionCancelledEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the email template for reactivated subscription notifications in the settings repository.
    /// </summary>
    /// <param name="emailTemplate">
    /// The email template to be updated for reactivated subscription notifications.
    /// </param>
    /// <param name="token">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating
    /// whether the operation was successful or encountered an error.
    /// </returns>
    Task<Result<Unit>> UpdateSubscriptionReActivatedEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the trial-ending-soon email template.
    /// </summary>
    Task<Result<Unit>> UpdateTrialEndingSoonEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the password-changed confirmation email template.
    /// </summary>
    Task<Result<Unit>> UpdatePasswordChangedEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Updates the "forgot password completed" email template in the settings repository.
    /// </summary>
    /// <param name="emailTemplate">
    /// The "forgot password completed" email template to be updated.
    /// </param>
    /// <param name="token">
    /// The cancellation token to listen for cancellations.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating
    /// the success or failure of the operation.
    /// </returns>
    Task<Result<Unit>> UpdateForgotPasswordCompletedEmailTemplateAsync(string emailTemplate, CancellationToken token);

    /// <summary>
    /// Retrieves the verification email template from the settings repository.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{String}"/>
    /// holding the current verification email template, or an error indicating the failure of the operation.
    /// </returns>
    Task<Result<string>> GetVerificationEmailTemplateAsync();

    /// <summary>
    /// Retrieves the password changed email template from the settings repository.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{String}"/>
    /// holding the current password changed email template, or an error indicating the failure of the operation.
    /// </returns>
    Task<Result<string>> GetRecoverPasswordEmailTemplateAsync();

    /// <summary>
    /// Retrieves the welcome email template for newly registered users.
    /// </summary>
    Task<Result<string>> GetUserWelcomeEmailTemplateAsync();

    /// <summary>
    /// Retrieves the subscription thank-you email template.
    /// </summary>
    Task<Result<string>> GetSubscriptionThankYouEmailTemplateAsync();

    /// <summary>
    /// Retrieves the trial-ending-soon email template.
    /// </summary>
    Task<Result<string>> GetTrialEndingSoonEmailTemplateAsync();

    /// <summary>
    /// Retrieves the password-changed confirmation email template.
    /// </summary>
    Task<Result<string>> GetPasswordChangedEmailTemplateAsync();

    /// <summary>
    /// Retrieves the subscription cancellation email template from the settings repository.
    /// </summary>
    /// <param name="token">The cancellation token to listen for cancellations.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a string representing
    /// the subscription cancellation email template.
    /// </returns>
    Task<Result<string>> GetSubscriptionCancelledEmailTemplateAsync();

    /// <summary>
    /// Retrieves the email template used for sending notifications when a subscription is reactivated.
    /// </summary>
    /// <param name="token">The cancellation token to listen for cancellations.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task result contains a <see cref="Result{EmailTemplate}"/>
    /// that includes the email template details if the operation is successful.
    /// </returns>
    Task<Result<string>> GetSubscriptionReActivatedEmailTemplateAsync();

    
}
