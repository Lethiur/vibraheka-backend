using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class GetTemplatesAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldGetVerificationTemplateAfterUpdate()
    {
        // Given: un template de verificacion previamente actualizado en SSM.
        string expectedTemplate = $"verification-template-{_faker.Random.Guid()}";
        await _service.ChangeEmailForVerificationAsync(expectedTemplate, CancellationToken.None);

        // When: se consulta el template de verificacion.
        Result<string> result = await _service.GetVerificationEmailTemplateAsync(CancellationToken.None);

        // Then: debe devolverse el valor actualizado.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }

    [Test]
    public async Task ShouldGetRecoverPasswordEmailTemplateAfterUpdate()
    {
        // Given: un template de recuperacion previamente actualizado en SSM.
        string expectedTemplate = $"password-template-{_faker.Random.Guid()}";
        await _service.ChangeRecoverPasswordEmailTemplateAsync(expectedTemplate, CancellationToken.None);

        // When: se consulta el template de recuperacion.
        Result<string> result = await _service.GetRecoverPasswordEmailTemplateAsync(CancellationToken.None);

        // Then: debe devolverse el valor actualizado.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedTemplate));
    }

    [Test]
    public void ShouldThrowWhenVerificationTemplateReadIsCancelled()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When / Then: la lectura debe lanzar OperationCanceledException.
        Assert.ThrowsAsync<OperationCanceledException>(() => _service.GetVerificationEmailTemplateAsync(cts.Token));
    }

    [Test]
    public void ShouldThrowWhenRecoverPasswordTemplateReadIsCancelled()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When / Then: la lectura debe lanzar OperationCanceledException.
        Assert.ThrowsAsync<OperationCanceledException>(() => _service.GetRecoverPasswordEmailTemplateAsync(cts.Token));
    }

    [Test]
    public async Task ShouldMapParameterNotFoundToInvalidVerificationTemplate()
    {
        // Given: un servicio con namespace inexistente para forzar ParameterNotFound.
        SettingsService service = CreateServiceWithRandomNamespace();

        // When: se intenta leer el template de verificacion sin parametro creado.
        Result<string> result = await service.GetVerificationEmailTemplateAsync(CancellationToken.None);

        // Then: debe mapearse a error de template de verificacion invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    public async Task ShouldMapParameterNotFoundToInvalidRecoverPasswordTemplate()
    {
        // Given: un servicio con namespace inexistente para forzar ParameterNotFound.
        SettingsService service = CreateServiceWithRandomNamespace();

        // When: se intenta leer el template de recuperacion sin parametro creado.
        Result<string> result = await service.GetRecoverPasswordEmailTemplateAsync(CancellationToken.None);

        // Then: debe mapearse a error de template de recuperacion invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidRecoverPasswordEmailTemplate));
    }

    private SettingsService CreateServiceWithRandomNamespace()
    {
        AWSConfig config = new()
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

        SettingsRepository repository = new(_ssmClient, config, CreateTestLogger<SettingsRepository>());
        return new SettingsService(
            repository,
            new StaticOptionsMonitor<AppSettingsEntity>(_appSettings),
            CreateTestLogger<SettingsService>());
    }

    private sealed class StaticOptionsMonitor<T>(T value) : Microsoft.Extensions.Options.IOptionsMonitor<T>
    {
        public T CurrentValue { get; private set; } = value;
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
