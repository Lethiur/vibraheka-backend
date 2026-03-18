using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class GetTemplatesForActionsTest : GenericSettingsServiceTest
{
    [Test]
    public void ShouldReturnCurrentStateOfTemplatesFromMemory()
    {
        // Given
        const string verificationTemplate = "verification-memory-value";
        const string recoverPasswordEmailTemplate = "recover-password-memory-value";
        const string welcomeTemplate = "welcome-memory-value";
        const string subscriptionThankYouTemplate = "subscription-thank-you-memory-value";
        const string trialEndingSoonTemplate = "trial-ending-soon-memory-value";
        const string passwordChangedTemplate = "password-changed-memory-value";

        _appSettings.VerificationEmailTemplate = verificationTemplate;
        _appSettings.RecoverPasswordEmailTemplate = recoverPasswordEmailTemplate;
        _appSettings.UserWelcomeEmailTemplate = welcomeTemplate;
        _appSettings.SubscriptionThankYouEmailTemplate = subscriptionThankYouTemplate;
        _appSettings.TrialEndingSoonEmailTemplate = trialEndingSoonTemplate;
        _appSettings.PasswordChangedEmailTemplate = passwordChangedTemplate;

        // When
        Result<IEnumerable<TemplateForActionEntity>> result = _service.GetAllTemplatesForActions();

        // Then
        Assert.That(result.IsSuccess, Is.True);
        List<TemplateForActionEntity> templates = result.Value.ToList();
        Assert.That(templates, Has.Count.EqualTo(6));
        Assert.That(templates.Any(t => t.ActionType == ActionType.UserVerification && t.TemplateID == verificationTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.PasswordReset && t.TemplateID == recoverPasswordEmailTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.UserRegistered && t.TemplateID == welcomeTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.SubscriptionThankYou && t.TemplateID == subscriptionThankYouTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.TrialEndingSoon && t.TemplateID == trialEndingSoonTemplate), Is.True);
        Assert.That(templates.Any(t => t.ActionType == ActionType.PasswordChanged && t.TemplateID == passwordChangedTemplate), Is.True);
    }
}
