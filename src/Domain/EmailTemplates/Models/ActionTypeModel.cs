using VibraHeka.Domain.User.Enums;

namespace VibraHeka.Domain.EmailTemplates.Models;

public class ActionTypeModel
{
    public static readonly Dictionary<ActionType, string> ActionTypes = new Dictionary<ActionType, string>
    {
        { ActionType.ForgotPasswordCompleted , "ForgotPasswordCompletedEmailTemplate"},
        { ActionType.UserVerification , "VerificationEmailTemplate"},
        { ActionType.PasswordReset , "RecoverPasswordEmailTemplate"},
        { ActionType.UserRegistered , "UserWelcomeEmailTemplate"},
        { ActionType.SubscriptionThankYou , "SubscriptionThankYouEmailTemplate"},
        { ActionType.TrialEndingSoon , "TrialEndingSoonEmailTemplate"},
        { ActionType.PasswordChanged , "PasswordChangedEmailTemplate"},
        { ActionType.SubscriptionCancelled , "SubscriptionCancelledEmailTemplate"},
        { ActionType.SubscriptionReactivated , "SubscriptionReActivatedEmailTemplate"},
    };
}
