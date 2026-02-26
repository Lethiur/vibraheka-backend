using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
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
    AppSettingsEntity appSettings,
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
    public async Task<Result<Unit>> ChangeEmailForResetPasswordAsync(string emailTemplate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(emailTemplate))
        {
            return Result.Failure<Unit>(SettingsErrors.InvalidPasswordChangedTemplate);
        }

        try
        {
            Result<Unit> repositoryResult =
                await repository.UpdatePasswordChangedTemplateAsync(emailTemplate, cancellationToken);

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
    public async Task<Result<string>> GetPasswordChangedTemplateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Result<string> repositoryResult = await repository.GetPasswordChangedTemplateAsync();
        if (repositoryResult.IsFailure)
        {
            return Result.Failure<string>(MapInfrastructureErrorForGet(repositoryResult.Error, true));
        }

        return repositoryResult;
    }

    /// <summary>
    /// Retrieves all templates used for actions.
    /// </summary>
    /// <returns>A <see cref="Result{IEnumerable{TemplateForActionEntity}}"/> containing available templates.</returns>
    public Result<IEnumerable<TemplateForActionEntity>> GetAllTemplatesForActions()
    {
        List<TemplateForActionEntity> templates =
        [
            new()
            {
                TemplateID = appSettings.VerificationEmailTemplate, ActionType = ActionType.UserVerification
            },
            new()
            {
                TemplateID = appSettings.PasswordChangedTemplate, ActionType = ActionType.PasswordReset
            }
        ];

        return templates;
    }

    /// <summary>
    /// Maps infrastructure repository errors to domain errors for update operations.
    /// </summary>
    /// <param name="infrastructureError">The error code returned by the repository.</param>
    /// <param name="isPasswordChangedTemplate">Indicates whether the error belongs to the password changed template flow.</param>
    /// <returns>A domain-level error code.</returns>
    private static string MapInfrastructureErrorForUpdate(string infrastructureError, bool isPasswordChangedTemplate)
    {
        return infrastructureError switch
        {
            InfrastructureConfigErrors.ParameterLimitExceeded => isPasswordChangedTemplate
                ? SettingsErrors.PasswordChangedTemplateUpdateFailed
                : SettingsErrors.VerificationEmailTemplateUpdateFailed,

            InfrastructureConfigErrors.TooManyUpdates => isPasswordChangedTemplate
                ? SettingsErrors.PasswordChangedTemplateUpdateFailed
                : SettingsErrors.VerificationEmailTemplateUpdateFailed,
            _ => SettingsErrors.GenericError
        };
    }

    /// <summary>
    /// Maps infrastructure repository errors to domain errors for retrieval operations.
    /// </summary>
    /// <param name="infrastructureError">The error code returned by the repository.</param>
    /// <param name="isPasswordChangedTemplate">Indicates whether the error belongs to the password changed template flow.</param>
    /// <returns>A domain-level error code.</returns>
    private static string MapInfrastructureErrorForGet(string infrastructureError, bool isPasswordChangedTemplate)
    {
        return infrastructureError switch
        {
            InfrastructureConfigErrors.ParameterNotFound => isPasswordChangedTemplate
                ? SettingsErrors.InvalidPasswordChangedTemplate
                : SettingsErrors.InvalidVerificationEmailTemplate,
            InfrastructureConfigErrors.AccessDenied => SettingsErrors.GenericError,
            AppErrors.GenericError => SettingsErrors.GenericError,
            _ => SettingsErrors.GenericError
        };
    }
}
