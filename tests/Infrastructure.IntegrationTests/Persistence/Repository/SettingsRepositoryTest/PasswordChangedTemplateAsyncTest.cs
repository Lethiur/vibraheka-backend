using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class RecoverPasswordEmailTemplateAsyncTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldUpdateRecoverPasswordEmailTemplateSuccessfully()
    {
        // Given: un template valido para recuperar password.
        string emailTemplate = $"<html><body>Password Changed {_faker.Random.Guid()}</body></html>";

        // When: se actualiza el parametro en SSM.
        Result<Unit> result = await Repository.UpdateRecoverPasswordEmailTemplateAsync(emailTemplate, CancellationToken.None);

        // Then: la operacion debe ser exitosa y quedar persistida.
        Assert.That(result.IsSuccess, Is.True);

        GetParameterResponse response = await SSMClient.GetParameterAsync(new GetParameterRequest
        {
            Name = RecoverPasswordParameterName
        });

        Assert.That(response.Parameter.Value, Is.EqualTo(emailTemplate));
    }

    [Test]
    public async Task ShouldGetRecoverPasswordEmailTemplateSuccessfully()
    {
        // Given: un template existente para recover password.
        string expectedTemplate = $"password-template-{_faker.Random.Guid()}";
        await SSMClient.PutParameterAsync(new PutParameterRequest
        {
            Name = RecoverPasswordParameterName,
            Value = expectedTemplate,
            Type = ParameterType.String,
            Overwrite = true
        });

        // When: se consulta el parametro por repositorio.
        Result<string> result = await Repository.GetRecoverPasswordEmailTemplateAsync();

        // Then: debe devolverse el valor guardado.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }

    [Test]
    public async Task ShouldReturnParameterNotFoundWhenRecoverPasswordParameterDoesNotExist()
    {
        // Given: un repositorio con namespace aleatorio sin parametros creados.
        SettingsRepository repository = new(SSMClient, BuildConfigWithRandomNamespace(), CreateTestLogger<SettingsRepository>());

        // When: se intenta obtener el template inexistente.
        Result<string> result = await repository.GetRecoverPasswordEmailTemplateAsync();

        // Then: debe devolverse ParameterNotFound.
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
