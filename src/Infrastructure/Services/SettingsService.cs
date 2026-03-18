using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Service class responsible for managing application settings, including
/// operations related to email templates.
/// </summary>
public class SettingsService(
    ISettingsRepository repository,
    IOptionsMonitor<AppSettingsEntity> appSettingsMonitor,
    ILogger<SettingsService> Logger) : ISettingsService
{
    /// <summary>
    /// Updates the email template used for verification purposes.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used for verification.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating whether the operation succeeded.</returns>
    public async Task<Result<Unit>> ChangeEmailForVerificationAsync(string emailTemplate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(emailTemplate))
        {
            return Result.Failure<Unit>(SettingsErrors.InvalidVerificationEmailTemplate);
        }

        try
        {
            Result<Unit> repositoryResult =
                await repository.UpdateVerificationEmailTemplateAsync(emailTemplate, cancellationToken);

            return repositoryResult.IsFailure
                ? Result.Failure<Unit>(MapInfrastructureErrorForUpdate(repositoryResult.Error, false))
                : Result.Success(Unit.Value);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Error while updating the email template for verification");
            return Result.Failure<Unit>(SettingsErrors.GenericError);
        }
    }

    /// <summary>
    /// Updates the email template used when a password is changed.
    /// </summary>
    /// <param name="emailTemplate">The new email template to be used after password changes.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating whether the operation succeeded.</returns>
    public async Task<Result<Unit>> ChangeRecoverPasswordEmailTemplateAsync(string emailTemplate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(emailTemplate))
        {
            return Result.Failure<Unit>(SettingsErrors.InvalidRecoverPasswordEmailTemplate);
        }

        try
        {
            Result<Unit> repositoryResult =
                await repository.UpdateRecoverPasswordEmailTemplateAsync(emailTemplate, cancellationToken);

            return repositoryResult.IsFailure
                ? Result.Failure<Unit>(MapInfrastructureErrorForUpdate(repositoryResult.Error, true))
                : Result.Success(Unit.Value);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Error while updating the email template for password reset");
            return Result.Failure<Unit>(SettingsErrors.GenericError);
        }
    }

    public Task<Result<Unit>> ChangeUserWelcomeEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken)
    {
        return UpdateTemplateAsync(
            emailTemplate,
            SettingsErrors.GenericError,
            token => repository.UpdateUserWelcomeEmailTemplateAsync(emailTemplate, token),
            cancellationToken,
            "Error while updating the user welcome email template");
    }

    public Task<Result<Unit>> ChangeSubscriptionThankYouEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken)
    {
        return UpdateTemplateAsync(
            emailTemplate,
            SettingsErrors.GenericError,
            token => repository.UpdateSubscriptionThankYouEmailTemplateAsync(emailTemplate, token),
            cancellationToken,
            "Error while updating the subscription thank-you email template");
    }

    public Task<Result<Unit>> ChangeTrialEndingSoonEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken)
    {
        return UpdateTemplateAsync(
            emailTemplate,
            SettingsErrors.GenericError,
            token => repository.UpdateTrialEndingSoonEmailTemplateAsync(emailTemplate, token),
            cancellationToken,
            "Error while updating the trial ending soon email template");
    }

    public Task<Result<Unit>> ChangePasswordChangedEmailTemplateAsync(string emailTemplate, CancellationToken cancellationToken)
    {
        return UpdateTemplateAsync(
            emailTemplate,
            SettingsErrors.GenericError,
            token => repository.UpdatePasswordChangedEmailTemplateAsync(emailTemplate, token),
            cancellationToken,
            "Error while updating the password changed email template");
    }

    /// <summary>
    /// Retrieves the verification email template.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{String}"/> containing the template or a domain-level error.</returns>
    public async Task<Result<string>> GetVerificationEmailTemplateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Result<string> repositoryResult = await repository.GetVerificationEmailTemplateAsync();
        if (repositoryResult.IsFailure)
        {
            return Result.Failure<string>(MapInfrastructureErrorForGet(repositoryResult.Error, false));
        }

        return repositoryResult;
    }

    /// <summary>
    /// Retrieves the password changed email template.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{String}"/> containing the template or a domain-level error.</returns>
    public async Task<Result<string>> GetRecoverPasswordEmailTemplateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Result<string> repositoryResult = await repository.GetRecoverPasswordEmailTemplateAsync();
        if (repositoryResult.IsFailure)
        {
            return Result.Failure<string>(MapInfrastructureErrorForGet(repositoryResult.Error, true));
        }

        return repositoryResult;
    }

    public Task<Result<string>> GetUserWelcomeEmailTemplateAsync(CancellationToken cancellationToken)
    {
        return GetTemplateAsync(
            cancellationToken,
            repository.GetUserWelcomeEmailTemplateAsync,
            SettingsErrors.GenericError);
    }

    public Task<Result<string>> GetSubscriptionThankYouEmailTemplateAsync(CancellationToken cancellationToken)
    {
        return GetTemplateAsync(
            cancellationToken,
            repository.GetSubscriptionThankYouEmailTemplateAsync,
            SettingsErrors.GenericError);
    }

    public Task<Result<string>> GetTrialEndingSoonEmailTemplateAsync(CancellationToken cancellationToken)
    {
        return GetTemplateAsync(
            cancellationToken,
            repository.GetTrialEndingSoonEmailTemplateAsync,
            SettingsErrors.GenericError);
    }

    public Task<Result<string>> GetPasswordChangedEmailTemplateAsync(CancellationToken cancellationToken)
    {
        return GetTemplateAsync(
            cancellationToken,
            repository.GetPasswordChangedEmailTemplateAsync,
            SettingsErrors.GenericError);
    }

    /// <summary>
    /// Retrieves all templates used for actions.
    /// </summary>
    /// <returns>A <see cref="Result{IEnumerable{TemplateForActionEntity}}"/> containing available templates.</returns>
    public Result<IEnumerable<TemplateForActionEntity>> GetAllTemplatesForActions()
    {
        AppSettingsEntity appSettings = appSettingsMonitor.CurrentValue;

        List<TemplateForActionEntity> templates =
        [
            new()
            {
                TemplateID = appSettings.VerificationEmailTemplate, ActionType = ActionType.UserVerification
            },
            new()
            {
                TemplateID = appSettings.RecoverPasswordEmailTemplate, ActionType = ActionType.PasswordReset
            },
            new()
            {
                TemplateID = appSettings.UserWelcomeEmailTemplate, ActionType = ActionType.UserRegistered
            },
            new()
            {
                TemplateID = appSettings.SubscriptionThankYouEmailTemplate, ActionType = ActionType.SubscriptionThankYou
            },
            new()
            {
                TemplateID = appSettings.TrialEndingSoonEmailTemplate, ActionType = ActionType.TrialEndingSoon
            },
            new()
            {
                TemplateID = appSettings.PasswordChangedEmailTemplate, ActionType = ActionType.PasswordChanged
            }
        ];

        return templates;
    }

    /// <summary>
    /// Maps infrastructure repository errors to domain errors for update operations.
    /// </summary>
    /// <param name="infrastructureError">The error code returned by the repository.</param>
    /// <param name="isRecoverPasswordEmailTemplate">Indicates whether the error belongs to the password changed template flow.</param>
    /// <returns>A domain-level error code.</returns>
    private static string MapInfrastructureErrorForUpdate(string infrastructureError, bool isRecoverPasswordEmailTemplate)
    {
        return infrastructureError switch
        {
            InfrastructureConfigErrors.ParameterLimitExceeded => isRecoverPasswordEmailTemplate
                ? SettingsErrors.RecoverPasswordEmailTemplateUpdateFailed
                : SettingsErrors.VerificationEmailTemplateUpdateFailed,

            InfrastructureConfigErrors.TooManyUpdates => isRecoverPasswordEmailTemplate
                ? SettingsErrors.RecoverPasswordEmailTemplateUpdateFailed
                : SettingsErrors.VerificationEmailTemplateUpdateFailed,
            _ => SettingsErrors.GenericError
        };
    }

    /// <summary>
    /// Maps infrastructure repository errors to domain errors for retrieval operations.
    /// </summary>
    /// <param name="infrastructureError">The error code returned by the repository.</param>
    /// <param name="isRecoverPasswordEmailTemplate">Indicates whether the error belongs to the password changed template flow.</param>
    /// <returns>A domain-level error code.</returns>
    private static string MapInfrastructureErrorForGet(string infrastructureError, bool isRecoverPasswordEmailTemplate)
    {
        return infrastructureError switch
        {
            InfrastructureConfigErrors.ParameterNotFound => isRecoverPasswordEmailTemplate
                ? SettingsErrors.InvalidRecoverPasswordEmailTemplate
                : SettingsErrors.InvalidVerificationEmailTemplate,
            _ => SettingsErrors.GenericError
        };
    }

    private async Task<Result<Unit>> UpdateTemplateAsync(
        string templateId,
        string invalidTemplateError,
        Func<CancellationToken, Task<Result<Unit>>> repositoryCall,
        CancellationToken cancellationToken,
        string logOnException)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            return Result.Failure<Unit>(invalidTemplateError);
        }

        try
        {
            Result<Unit> repositoryResult = await repositoryCall(cancellationToken);
            return repositoryResult.IsFailure
                ? Result.Failure<Unit>(SettingsErrors.GenericError)
                : Result.Success(Unit.Value);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "{Message}", logOnException);
            return Result.Failure<Unit>(SettingsErrors.GenericError);
        }
    }

    private async Task<Result<string>> GetTemplateAsync(
        CancellationToken cancellationToken,
        Func<Task<Result<string>>> repositoryCall,
        string fallbackError)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Result<string> repositoryResult = await repositoryCall();
        return repositoryResult.IsFailure
            ? Result.Failure<string>(fallbackError)
            : repositoryResult;
    }
}
