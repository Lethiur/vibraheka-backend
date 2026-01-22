using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Service class responsible for managing application settings, including
/// operations related to the verification email template.
/// </summary>
public class SettingsService(ISettingsRepository Repository, AppSettingsEntity appSettings) : ISettingsService
{
    /// <summary>
    /// Updates the email template used for verification purposes.
    /// Validates the provided email string and updates the template in the repository if valid.
    /// </summary>
    /// <param name="email">The new email template to be used for verification.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the tasks</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating the success or failure of the operation,
    /// along with potential error details.</returns>
    public async Task<Result<Unit>> ChangeEmailForVerificationAsync(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Unit>(SettingsErrors.InvalidVerificationEmailTemplate);
        }

        Result<Unit> updateVerificationEmailTemplateAsync =
            await Repository.UpdateVerificationEmailTemplateAsync(email, cancellationToken);

        updateVerificationEmailTemplateAsync.OnSuccessTry(_ => appSettings.VerificationEmailTemplate = email);

        return updateVerificationEmailTemplateAsync;
    }

    /// <summary>
    /// Retrieves all templates used for actions.
    /// Fetches all available instances of <see cref="TemplateForActionEntity"/> from the repository.
    /// </summary>
    /// <returns>A <see>
    ///         <cref>Result{IEnumerable{TemplateForActionEntity}}</cref>
    ///     </see>
    ///     containing a collection of templates or an error if the operation fails.</returns>
    public Result<IEnumerable<TemplateForActionEntity>> GetAllTemplatesForActions()
    {
        List<TemplateForActionEntity> templates =
        [
            new TemplateForActionEntity()
            {
                TemplateID = appSettings.VerificationEmailTemplate, ActionType = ActionType.UserVerification
            },
            new TemplateForActionEntity()
            {
                TemplateID = appSettings.EmailForResetPassword, ActionType = ActionType.PasswordReset
            }
        ];
        
        return templates;
    }
}
