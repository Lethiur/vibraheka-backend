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

        // When: se actualiza el template de verificacion.
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(_faker.Random.Guid(), CancellationToken.None);

        // Then: la operacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task ShouldReturnFailureWhenVerificationTemplateIsInvalid()
    {
        // Given: un template invalido (null o whitespace).

        // When: se intenta actualizar el template de verificacion.
        Result<Unit> result = await _service.ChangeEmailForVerificationAsync(Guid.Empty, CancellationToken.None);

        // Then: debe devolverse error de template invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidVerificationEmailTemplate));
    }

    [Test]
    public async Task ShouldUpdateRecoverPasswordEmailTemplateSuccessfully()
    {
        // When: se actualiza el template de recuperacion.
        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(_faker.Random.Guid(), CancellationToken.None);

        // Then: la operacion debe ser exitosa.
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRecoverPasswordEmailTemplateIsInvalid()
    {
        // When: se intenta actualizar el template de recuperacion.
        Result<Unit> result = await _service.ChangeRecoverPasswordEmailTemplateAsync(Guid.Empty, CancellationToken.None);

        // Then: debe devolverse error de template invalido.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(SettingsErrors.InvalidRecoverPasswordEmailTemplate));
    }
}
