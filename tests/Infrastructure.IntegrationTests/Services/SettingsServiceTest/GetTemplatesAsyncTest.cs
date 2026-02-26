using CSharpFunctionalExtensions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class GetTemplatesAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldGetVerificationTemplateAfterUpdate()
    {
        string expectedTemplate = $"verification-template-{_faker.Random.Guid()}";
        await _service.ChangeEmailForVerificationAsync(expectedTemplate, CancellationToken.None);

        Result<string> result = await _service.GetVerificationEmailTemplateAsync(CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }

    [Test]
    public async Task ShouldGetPasswordChangedTemplateAfterUpdate()
    {
        string expectedTemplate = $"password-template-{_faker.Random.Guid()}";
        await _service.ChangeEmailForResetPasswordAsync(expectedTemplate, CancellationToken.None);

        Result<string> result = await _service.GetPasswordChangedTemplateAsync(CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }
}
