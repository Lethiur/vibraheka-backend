using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class NewAdminEmailTemplatesAsyncTest : GenericSettingsRepositoryTest
{
    [TestCase("UserWelcome")]
    [TestCase("SubscriptionThankYou")]
    [TestCase("TrialEndingSoon")]
    [TestCase("PasswordChanged")]
    public async Task ShouldUpdateTemplateForNewAdminActions(string templateType)
    {
        // Given
        const string templateId = "template-id";
        string parameterName = GetParameterName(templateType);
        SsmClientMock
            .Setup(x => x.PutParameterAsync(
                It.Is<PutParameterRequest>(r => r.Name == parameterName && r.Value == templateId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutParameterResponse());

        // When
        Result<Unit> result = await UpdateTemplate(templateType, templateId);

        // Then
        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("UserWelcome")]
    [TestCase("SubscriptionThankYou")]
    [TestCase("TrialEndingSoon")]
    [TestCase("PasswordChanged")]
    public async Task ShouldGetTemplateForNewAdminActions(string templateType)
    {
        // Given
        const string expectedTemplate = "expected-template";
        string parameterName = GetParameterName(templateType);
        SsmClientMock
            .Setup(x => x.GetParameterAsync(
                It.Is<GetParameterRequest>(r => r.Name == parameterName && r.WithDecryption == true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter { Value = expectedTemplate }
            });

        // When
        Result<string> result = await GetTemplate(templateType);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }

    private Task<Result<Unit>> UpdateTemplate(string templateType, string templateId)
    {
        return templateType switch
        {
            "UserWelcome" => Repository.UpdateUserWelcomeEmailTemplateAsync(templateId, CancellationToken.None),
            "SubscriptionThankYou" => Repository.UpdateSubscriptionThankYouEmailTemplateAsync(templateId, CancellationToken.None),
            "TrialEndingSoon" => Repository.UpdateTrialEndingSoonEmailTemplateAsync(templateId, CancellationToken.None),
            "PasswordChanged" => Repository.UpdatePasswordChangedEmailTemplateAsync(templateId, CancellationToken.None),
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
