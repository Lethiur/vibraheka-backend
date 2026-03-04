using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;

public class ChangeTemplateForActionCommandHandler(
    ISettingsService SettingsService,
    ICurrentUserService CurrentUserService,
    IEmailTemplatesService EmailTemplatesService,
    ILogger<ChangeTemplateForActionCommandHandler> logger)
    : IRequestHandler<ChangeTemplateForActionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangeTemplateForActionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing command for changing template for action: {ActionType}", request.ActionType);
        return await Maybe.From(CurrentUserService.UserId)
            .Where(userID => !string.IsNullOrEmpty(userID) && !string.IsNullOrWhiteSpace(userID))
            .ToResult(UserErrors.InvalidUserID)
            .BindTry(_ => EmailTemplatesService.GetTemplateByID(request.TemplateID, cancellationToken))
            .BindTry(async _ =>
            {
                return request.ActionType switch
                {
                    ActionType.UserRegistered => await SettingsService.ChangeUserWelcomeEmailTemplateAsync(
                        request.TemplateID, cancellationToken),
                    ActionType.UserVerification => await SettingsService.ChangeEmailForVerificationAsync(
                        request.TemplateID, cancellationToken),
                    ActionType.PasswordReset => await SettingsService.ChangeRecoverPasswordEmailTemplateAsync(
                        request.TemplateID, cancellationToken),
                    ActionType.SubscriptionThankYou => await SettingsService.ChangeSubscriptionThankYouEmailTemplateAsync(
                        request.TemplateID, cancellationToken),
                    ActionType.TrialEndingSoon => await SettingsService.ChangeTrialEndingSoonEmailTemplateAsync(
                        request.TemplateID, cancellationToken),
                    ActionType.PasswordChanged => await SettingsService.ChangePasswordChangedEmailTemplateAsync(
                        request.TemplateID, cancellationToken),
                    _ => Result.Failure<Unit>(EmailTemplateErrors.InvalidAction)
                };
            });
    }
}
