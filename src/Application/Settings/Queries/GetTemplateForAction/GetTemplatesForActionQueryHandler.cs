using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.EmailTemplates.Entities;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Enums;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.Settings.Queries.GetTemplateForAction;

/// <summary>
/// Handles queries to retrieve a collection of templates associated with a specific action.
/// </summary>
/// <remarks>
/// This query handler is responsible for processing instances of <see cref="GetTemplatesForActionQuery"/>
/// and returning a list of <see cref="TemplateForActionEntity"/> objects that match the query criteria.
/// </remarks>
public class GetTemplatesForActionQueryHandler(
    ICurrentUserService CurrentUserService,
    IOptionsMonitor<AppSettingsEntity> AppSettings,
    ILogger<GetTemplatesForActionQueryHandler> Logger)
    : IRequestHandler<GetTemplatesForActionQuery, Result<IEnumerable<TemplateForActionEntity>>>
{
    public Task<Result<IEnumerable<TemplateForActionEntity>>> Handle(GetTemplatesForActionQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Maybe.From(CurrentUserService.UserId)
            .Where(userID =>
                !string.IsNullOrEmpty(userID) && !string.IsNullOrWhiteSpace(userID))
            .ToResult(UserErrors.InvalidUserID)
            .BindTry(_ => GetTemplateList(), exception =>
            {
                Logger.LogError(exception, "Problem retrieving templates for actions");
                return AppErrors.GenericError;
            }));

    }


    private Result<IEnumerable<TemplateForActionEntity>> GetTemplateList()
    {
        List<TemplateForActionEntity> templates =
        [
            new()
            {
                TemplateID = AppSettings.CurrentValue.VerificationEmailTemplate, ActionType = ActionType.UserVerification
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.RecoverPasswordEmailTemplate, ActionType = ActionType.PasswordReset
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.UserWelcomeEmailTemplate, ActionType = ActionType.UserRegistered
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.SubscriptionThankYouEmailTemplate, ActionType = ActionType.SubscriptionThankYou
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.TrialEndingSoonEmailTemplate, ActionType = ActionType.TrialEndingSoon
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.PasswordChangedEmailTemplate, ActionType = ActionType.PasswordChanged
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.ForgotPasswordCompletedEmailTemplate, ActionType = ActionType.ForgotPasswordCompleted
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.SubscriptionCancelledEmailTemplate, ActionType = ActionType.SubscriptionCancelled
            },
            new()
            {
                TemplateID = AppSettings.CurrentValue.SubscriptionReActivatedEmailTemplate, ActionType = ActionType.SubscriptionReactivated
            }
        ];

        return templates;

    }
    
}
