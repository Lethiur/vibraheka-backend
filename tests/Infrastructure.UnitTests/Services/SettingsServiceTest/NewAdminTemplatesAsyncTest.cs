using CSharpFunctionalExtensions;
using MediatR;
using Moq;

namespace VibraHeka.Infrastructure.UnitTests.Services.SettingsServiceTest;

[TestFixture]
public class NewAdminTemplatesAsyncTest : GenericSettingsServiceTest
{
    [TestCase("UserWelcome")]
    [TestCase("SubscriptionThankYou")]
    [TestCase("TrialEndingSoon")]
    [TestCase("PasswordChanged")]
    public async Task ShouldUpdateNewAdminTemplatesSuccessfully(string templateType)
    {
        // Given
        const string templateId = "template-123";
        SetupUpdateSuccess(templateType, templateId);

        // When
        Result<Unit> result = await UpdateTemplate(templateType, templateId);

        // Then
        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("UserWelcome")]
    [TestCase("SubscriptionThankYou")]
    [TestCase("TrialEndingSoon")]
    [TestCase("PasswordChanged")]
    public async Task ShouldReturnGenericErrorWhenTemplateIdIsInvalid(string templateType)
    {
        // Given

        // When
        Result<Unit> result = await UpdateTemplate(templateType, "   ");

        // Then
        Assert.That(result.IsFailure, Is.True);
    }

    [TestCase("UserWelcome")]
    [TestCase("SubscriptionThankYou")]
    [TestCase("TrialEndingSoon")]
    [TestCase("PasswordChanged")]
    public async Task ShouldGetNewAdminTemplatesSuccessfully(string templateType)
    {
        // Given
        const string expectedTemplate = "template-id";
        SetupGetSuccess(templateType, expectedTemplate);

        // When
        Result<string> result = await GetTemplate(templateType);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }

    private void SetupUpdateSuccess(string templateType, string templateId)
    {
        switch (templateType)
        {
            case "UserWelcome":
                RepositoryMock.Setup(x => x.UpdateUserWelcomeEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Success(Unit.Value));
                break;
            case "SubscriptionThankYou":
                RepositoryMock.Setup(x => x.UpdateSubscriptionThankYouEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Success(Unit.Value));
                break;
            case "TrialEndingSoon":
                RepositoryMock.Setup(x => x.UpdateTrialEndingSoonEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Success(Unit.Value));
                break;
            case "PasswordChanged":
                RepositoryMock.Setup(x => x.UpdatePasswordChangedEmailTemplateAsync(templateId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Success(Unit.Value));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null);
        }
    }

    private void SetupGetSuccess(string templateType, string templateId)
    {
        switch (templateType)
        {
            case "UserWelcome":
                RepositoryMock.Setup(x => x.GetUserWelcomeEmailTemplateAsync())
                    .Returns(Task.FromResult(Result.Success(templateId)));
                break;
            case "SubscriptionThankYou":
                RepositoryMock.Setup(x => x.GetSubscriptionThankYouEmailTemplateAsync())
                    .Returns(Task.FromResult(Result.Success(templateId)));
                break;
            case "TrialEndingSoon":
                RepositoryMock.Setup(x => x.GetTrialEndingSoonEmailTemplateAsync())
                    .Returns(Task.FromResult(Result.Success(templateId)));
                break;
            case "PasswordChanged":
                RepositoryMock.Setup(x => x.GetPasswordChangedEmailTemplateAsync())
                    .Returns(Task.FromResult(Result.Success(templateId)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null);
        }
    }

    private Task<Result<Unit>> UpdateTemplate(string templateType, string templateId)
    {
        return templateType switch
        {
            "UserWelcome" => Service.ChangeUserWelcomeEmailTemplateAsync(templateId, CancellationToken.None),
            "SubscriptionThankYou" => Service.ChangeSubscriptionThankYouEmailTemplateAsync(templateId, CancellationToken.None),
            "TrialEndingSoon" => Service.ChangeTrialEndingSoonEmailTemplateAsync(templateId, CancellationToken.None),
            "PasswordChanged" => Service.ChangePasswordChangedEmailTemplateAsync(templateId, CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };
    }

    private Task<Result<string>> GetTemplate(string templateType)
    {
        return templateType switch
        {
            "UserWelcome" => Service.GetUserWelcomeEmailTemplateAsync(CancellationToken.None),
            "SubscriptionThankYou" => Service.GetSubscriptionThankYouEmailTemplateAsync(CancellationToken.None),
            "TrialEndingSoon" => Service.GetTrialEndingSoonEmailTemplateAsync(CancellationToken.None),
            "PasswordChanged" => Service.GetPasswordChangedEmailTemplateAsync(CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, null)
        };
    }
}
