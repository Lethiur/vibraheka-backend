using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class GetVerificationEmailTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldOverwriteExistingParameterWhenUpdating()
    {
        string firstTemplate = "First Template";
        string secondTemplate = "Second Template (Updated)";

        await SSMClient.PutParameterAsync(new PutParameterRequest
        {
            Name = VerificationParameterName,
            Value = firstTemplate,
            Type = ParameterType.String,
            Overwrite = true
        });

        Result<Unit> updateResult =
            await Repository.UpdateVerificationEmailTemplateAsync(secondTemplate, CancellationToken.None);
        Result<string> getResult = await Repository.GetVerificationEmailTemplateAsync();

        Assert.That(updateResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(secondTemplate));
    }
}
