using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class PasswordChangedTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldUpdatePasswordChangedTemplateSuccessfully()
    {
        string emailTemplate = $"<html><body>Password Changed {_faker.Random.Guid()}</body></html>";

        Result<Unit> result = await Repository.UpdatePasswordChangedTemplateAsync(emailTemplate, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);

        GetParameterResponse response = await SSMClient.GetParameterAsync(new GetParameterRequest
        {
            Name = PasswordChangedParameterName
        });

        Assert.That(response.Parameter.Value, Is.EqualTo(emailTemplate));
    }

    [Test]
    public async Task ShouldGetPasswordChangedTemplateSuccessfully()
    {
        string expectedTemplate = $"password-template-{_faker.Random.Guid()}";
        await SSMClient.PutParameterAsync(new PutParameterRequest
        {
            Name = PasswordChangedParameterName,
            Value = expectedTemplate,
            Type = ParameterType.String,
            Overwrite = true
        });

        Result<string> result = await Repository.GetPasswordChangedTemplateAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }
}
