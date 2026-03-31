using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class GetAllTemplatesForActionsTest : GenericSettingsServiceTest
{

    [Test]
    public void ShouldReturnTemplatesMappingCorrectlyFromAppSettings()
    {
        // Given
        const string verificationTemplate = "verification-memory-value";
        const string recoverPasswordEmailTemplate = "recover-password-memory-value";
        const string welcomeTemplate = "welcome-memory-value";
        const string subscriptionThankYouTemplate = "subscription-thank-you-memory-value";
        const string trialEndingSoonTemplate = "trial-ending-soon-memory-value";
        const string passwordChangedTemplate = "password-changed-memory-value";
        const string subscriptionCancelledTemplate = "password-changed-memory-value";
        const string subscriptionReactivatedTemplate = "password-changed-memory-value";
        const string forgotPasswordCompleted = "password-changed-memory-value";

        AppSettings.VerificationEmailTemplate = verificationTemplate;
        AppSettings.RecoverPasswordEmailTemplate = recoverPasswordEmailTemplate;
        AppSettings.UserWelcomeEmailTemplate = welcomeTemplate;
        AppSettings.SubscriptionThankYouEmailTemplate = subscriptionThankYouTemplate;
        AppSettings.TrialEndingSoonEmailTemplate = trialEndingSoonTemplate;
        AppSettings.PasswordChangedEmailTemplate = passwordChangedTemplate;
        AppSettings.SubscriptionCancelledEmailTemplate = subscriptionCancelledTemplate;
        AppSettings.SubscriptionReActivatedEmailTemplate = subscriptionReactivatedTemplate;
        AppSettings.ForgotPasswordCompletedEmailTemplate = forgotPasswordCompleted;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = Service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        List<TemplateForActionEntity> templates = result.Value.ToList();
        Assert.That(templates, Has.Count.EqualTo(9));
        Assert.That(templates.Any(t => t.ActionType == ActionType.UserVerification && t.TemplateID == verificationTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.PasswordReset && t.TemplateID == recoverPasswordEmailTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.UserRegistered && t.TemplateID == welcomeTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.SubscriptionThankYou && t.TemplateID == subscriptionThankYouTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.TrialEndingSoon && t.TemplateID == trialEndingSoonTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.PasswordChanged && t.TemplateID == passwordChangedTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.SubscriptionCancelled && t.TemplateID == subscriptionCancelledTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.SubscriptionReactivated && t.TemplateID == subscriptionReactivatedTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.ForgotPasswordCompleted && t.TemplateID == forgotPasswordCompleted), Is.True);
    }

    [Test]
    public void ShouldReturnEmptyValuesWhenAppSettingsAreEmpty()
    {
        // Given
        AppSettings.VerificationEmailTemplate = string.Empty;
        AppSettings.RecoverPasswordEmailTemplate = string.Empty;
        AppSettings.UserWelcomeEmailTemplate = string.Empty;
        AppSettings.SubscriptionThankYouEmailTemplate = string.Empty;
        AppSettings.TrialEndingSoonEmailTemplate = string.Empty;
        AppSettings.PasswordChangedEmailTemplate = string.Empty;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = Service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.All(t => string.IsNullOrEmpty(t.TemplateID)), Is.True);
    }
}
