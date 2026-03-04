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
        const string verificationTemplate = "verification-id-123";
        const string resetPasswordTemplate = "reset-id-456";
        const string welcomeTemplate = "welcome-id-001";
        const string subscriptionThankYouTemplate = "sub-thanks-id-002";
        const string trialEndingSoonTemplate = "trial-ending-id-003";
        const string passwordChangedTemplate = "password-changed-id-004";

        AppSettings.VerificationEmailTemplate = verificationTemplate;
        AppSettings.RecoverPasswordEmailTemplate = resetPasswordTemplate;
        AppSettings.UserWelcomeEmailTemplate = welcomeTemplate;
        AppSettings.SubscriptionThankYouEmailTemplate = subscriptionThankYouTemplate;
        AppSettings.TrialEndingSoonEmailTemplate = trialEndingSoonTemplate;
        AppSettings.PasswordChangedEmailTemplate = passwordChangedTemplate;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = Service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        List<TemplateForActionEntity> templates = result.Value.ToList();

        Assert.That(templates, Has.Count.EqualTo(6));
        Assert.That(templates.Any(t => t.ActionType == ActionType.UserVerification && t.TemplateID == verificationTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.PasswordReset && t.TemplateID == resetPasswordTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.UserRegistered && t.TemplateID == welcomeTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.SubscriptionThankYou && t.TemplateID == subscriptionThankYouTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.TrialEndingSoon && t.TemplateID == trialEndingSoonTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.PasswordChanged && t.TemplateID == passwordChangedTemplate), Is.True);
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
