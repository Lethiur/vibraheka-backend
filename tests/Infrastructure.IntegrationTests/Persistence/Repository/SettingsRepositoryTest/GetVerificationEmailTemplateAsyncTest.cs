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
        // Given: un parametro inicial y luego un valor actualizado para verificacion.
        string firstTemplate = "First Template";
        string secondTemplate = "Second Template (Updated)";

        await SSMClient.PutParameterAsync(new PutParameterRequest
        {
            Name = VerificationParameterName,
            Value = firstTemplate,
            Type = ParameterType.String,
            Overwrite = true
        });

        // When: se actualiza y luego se consulta el parametro.
        Result<Unit> updateResult =
            await Repository.UpdateVerificationEmailTemplateAsync(secondTemplate, CancellationToken.None);
        Result<string> getResult = await Repository.GetVerificationEmailTemplateAsync();

        // Then: debe persistirse y devolverse el nuevo valor.
        Assert.That(updateResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(secondTemplate));
    }

    [Test]
    public async Task ShouldReturnParameterNotFoundWhenVerificationParameterDoesNotExist()
    {
        // Given: un repositorio apuntando a namespace aleatorio sin parametros creados.
        SettingsRepository repository = new(SSMClient, BuildConfigWithRandomNamespace(), CreateTestLogger<SettingsRepository>());

        // When: se intenta leer el template de verificacion inexistente.
        Result<string> result = await repository.GetVerificationEmailTemplateAsync();

        // Then: debe devolverse el error de parametro no encontrado.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(InfrastructureConfigErrors.ParameterNotFound));
    }

    private AWSConfig BuildConfigWithRandomNamespace()
    {
        return new AWSConfig
        {
            EmailTemplatesBucketName = _configuration.EmailTemplatesBucketName,
            UserCodesTable = _configuration.UserCodesTable,
            EmailTemplatesTable = _configuration.EmailTemplatesTable,
            UsersTable = _configuration.UsersTable,
#if DEBUG
            CodesTable = _configuration.CodesTable,
#endif
            ClientId = _configuration.ClientId,
            UserPoolId = _configuration.UserPoolId,
            Location = _configuration.Location,
            Profile = _configuration.Profile,
            PasswordResetTokenSecret = _configuration.PasswordResetTokenSecret,
            ActionLogTable = _configuration.ActionLogTable,
            SubscriptionTable = _configuration.SubscriptionTable,
            SubscriptionUserIdIndex = _configuration.SubscriptionUserIdIndex,
            SettingsNameSpace = $"integration-missing-{Guid.NewGuid():N}"
        };
    }
}
