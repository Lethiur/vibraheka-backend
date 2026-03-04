using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class NewAdminEmailTemplatesAsyncTest : GenericSettingsRepositoryTest
{
    [TestCase("welcome-template", "UserWelcome")]
    [TestCase("subscription-thank-you-template", "SubscriptionThankYou")]
    [TestCase("trial-ending-soon-template", "TrialEndingSoon")]
    [TestCase("password-changed-template", "PasswordChanged")]
    public async Task ShouldUpdateAndGetNewAdminManagedTemplates(string templateValue, string templateType)
    {
        // Given: a value and one of the new admin-managed template slots.
        string parameterName = GetParameterName(templateType);

        // When: saving template through repository.
        Result<Unit> saveResult = await SaveTemplate(templateType, templateValue);

        // Then: update succeeds and value is persisted in SSM.
        Assert.That(saveResult.IsSuccess, Is.True);
        GetParameterResponse rawResponse = await SSMClient.GetParameterAsync(new GetParameterRequest
        {
            Name = parameterName
        });
        Assert.That(rawResponse.Parameter.Value, Is.EqualTo(templateValue));

        // And: getting template through repository returns same value.
        Result<string> getResult = await GetTemplate(templateType);
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(templateValue));
    }

    private Task<Result<Unit>> SaveTemplate(string templateType, string templateValue)
    {
        return templateType switch
        {
            "UserWelcome" => Repository.UpdateUserWelcomeEmailTemplateAsync(templateValue, CancellationToken.None),
            "SubscriptionThankYou" => Repository.UpdateSubscriptionThankYouEmailTemplateAsync(templateValue, CancellationToken.None),
            "TrialEndingSoon" => Repository.UpdateTrialEndingSoonEmailTemplateAsync(templateValue, CancellationToken.None),
            "PasswordChanged" => Repository.UpdatePasswordChangedEmailTemplateAsync(templateValue, CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };
    }

    private Task<Result<string>> GetTemplate(string templateType)
    {
        return templateType switch
        {
            "UserWelcome" => Repository.GetUserWelcomeEmailTemplateAsync(),
            "SubscriptionThankYou" => Repository.GetSubscriptionThankYouEmailTemplateAsync(),
            "TrialEndingSoon" => Repository.GetTrialEndingSoonEmailTemplateAsync(),
            "PasswordChanged" => Repository.GetPasswordChangedEmailTemplateAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };
    }

    private string GetParameterName(string templateType)
    {
        return templateType switch
        {
            "UserWelcome" => UserWelcomeParameterName,
            "SubscriptionThankYou" => SubscriptionThankYouParameterName,
            "TrialEndingSoon" => TrialEndingSoonParameterName,
            "PasswordChanged" => PasswordChangedParameterName,
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };
    }
}
