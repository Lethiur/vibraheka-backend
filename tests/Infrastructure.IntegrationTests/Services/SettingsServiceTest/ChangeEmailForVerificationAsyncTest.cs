using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.SettingsServiceTest;

[TestFixture]
public class ChangeEmailForVerificationAsyncTest : GenericSettingsServiceTest
{
    [Test]
    public async Task ShouldUpdateVerificationTemplateSuccessfully()
    {
        // Given: un template de verificacion valido.
        string newTemplate = $"verification-template-{_faker.Random.Guid()}";

        // When: se actualiza el template de verificacion.
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(newTemplate, CancellationToken.None);

        // Then: la operacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null!)]
    public async Task ShouldReturnFailureWhenVerificationTemplateIsInvalid(string invalidTemplate)
    {
        // Given: un template invalido (null o whitespace).

        // When: se intenta actualizar el template de verificacion.
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(invalidTemplate, CancellationToken.None);

        // Then: debe devolverse error de template invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    public async Task ShouldUpdateRecoverPasswordEmailTemplateSuccessfully()
    {
        // Given: un template de recuperacion valido.
        string newTemplate = $"password-changed-template-{_faker.Random.Guid()}";

        // When: se actualiza el template de recuperacion.
        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(newTemplate, CancellationToken.None);

        // Then: la operacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null!)]
    public async Task ShouldReturnFailureWhenRecoverPasswordEmailTemplateIsInvalid(string invalidTemplate)
    {
        // Given: un template de recuperacion invalido (null o whitespace).

        // When: se intenta actualizar el template de recuperacion.
        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(invalidTemplate, CancellationToken.None);

        // Then: debe devolverse error de template invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidRecoverPasswordEmailTemplate));
    }

    [Test]
    public async Task ShouldReturnGenericErrorWhenVerificationTemplateExceedsSsmLimit()
    {
        // Given: un template excesivamente grande para provocar ParameterLimitExceeded en SSM.
        string tooLargeTemplate = new('A', 20000);

        // When: se intenta actualizar el template de verificacion con payload invalido.
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(tooLargeTemplate, CancellationToken.None);

        // Then: debe mapearse al error generico segun la respuesta real de AWS en integracion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.GenericError));
    }

    [Test]
    public async Task ShouldReturnGenericErrorWhenRecoverPasswordTemplateExceedsSsmLimit()
    {
        // Given: un template excesivamente grande para provocar ParameterLimitExceeded en SSM.
        string tooLargeTemplate = new('A', 20000);

        // When: se intenta actualizar el template de recuperacion con payload invalido.
        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(tooLargeTemplate, CancellationToken.None);

        // Then: debe mapearse al error generico segun la respuesta real de AWS en integracion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.GenericError));
    }
}
