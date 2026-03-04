using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SettingsRepositoryTest;

[TestFixture]
public class UpdateRecoverPasswordEmailTemplateTest : GenericSettingsRepositoryTest
{
    [Test]
    public async Task ShouldReturnGenericErrorWhenCancellationIsRequested()
    {
        // Given: un token de cancelacion ya cancelado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta actualizar el template de recover password con operacion cancelada.
        Result<Unit> result = await Repository.UpdateRecoverPasswordEmailTemplateAsync("template-cancelled", cts.Token);

        // Then: el repositorio debe mapear al error generico.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }
}
