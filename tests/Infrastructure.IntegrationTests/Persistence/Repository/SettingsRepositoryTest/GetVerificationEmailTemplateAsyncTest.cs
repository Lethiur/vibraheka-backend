using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class GetVerificationEmailTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldOverwriteExistingParameterWhenUpdating()
    {
        // Given: an initial parameter and then an updated value for verification.
        Guid firstTemplate = Guid.NewGuid();
        Guid secondTemplate = Guid.NewGuid();

        await SSMClient.PutParameterAsync(new PutParameterRequest
        {
            Name = VerificationParameterName,
            Value = firstTemplate.ToString(),
            Type = ParameterType.String,
            Overwrite = true
        });

        // When: updating and then retrieving the parameter.
        Result<Unit> updateResult =
            await Repository.UpdateVerificationEmailTemplateAsync(secondTemplate, CancellationToken.None);
        Result<string> getResult = await Repository.GetVerificationEmailTemplateAsync();

        // Then: should persist and return the new value.
        Assert.That(updateResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(secondTemplate.ToString()));
    }

    [Test]
    public async Task ShouldReturnParameterNotFoundWhenVerificationParameterDoesNotExist()
    {
        // Given: a repository pointing to a random namespace with no parameters created.
        SettingsRepository repository = new(SSMClient, BuildConfigWithRandomNamespace(), CreateTestLogger<SettingsRepository>());

        // When: trying to read the non-existent verification template.
        Result<string> result = await repository.GetVerificationEmailTemplateAsync();

        // Then: should return the parameter not found error.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.ParameterNotFound));
    }

    private AWSConfig BuildConfigWithRandomNamespace()
    {
        return new AWSConfig
        {
            EmailTemplatesBucketName = _configuration.EmailTemplatesBucketName,
            ClientId = _configuration.ClientId,
            UserPoolId = _configuration.UserPoolId,
            Location = _configuration.Location,
            Profile = _configuration.Profile,
            PasswordResetTokenSecret = _configuration.PasswordResetTokenSecret,
            Environment = "VibraHeka-test",
            SubscriptionUserIdIndex = _configuration.SubscriptionUserIdIndex,
            SettingsNameSpace = $"integration-missing-{Guid.NewGuid():N}"
        };
    }
}
