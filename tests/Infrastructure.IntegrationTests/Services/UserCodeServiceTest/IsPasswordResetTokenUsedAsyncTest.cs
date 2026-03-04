using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserCodeServiceTest;

[TestFixture]
public class IsPasswordResetTokenUsedAsyncTest : GenericUserCodeServiceIntegrationTest
{
    [Test]
    public async Task ShouldReturnTrueWhenReplayMarkerExistsInStorage()
    {
        // Given: un replay marker persistido para el token solicitado.
        string tokenId = Guid.NewGuid().ToString("N");
        string email = $"{tokenId}@test.com";
        await _userCodeRepository.SaveCode(new UserCodeEntity
        {
            UserEmail = email,
            ActionType = ActionType.PasswordReset,
            Code = tokenId,
            ExpiresAtUnix = DateTimeOffset.UtcNow.AddMinutes(20).ToUnixTimeSeconds(),
            CreatedBy = "integration-test",
            LastModifiedBy = "integration-test"
        }, CancellationToken.None);

        // When: el servicio valida si ese token ya fue consumido.
        Result<bool> result = await _userCodeService.IsPasswordResetTokenUsedAsync(
            email,
            tokenId,
            CancellationToken.None);

        // Then: debe retornar true porque existe marker en persistencia.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);

        await CleanupTokenAsync(tokenId);
    }

    [Test]
    public async Task ShouldReturnFalseWhenReplayMarkerDoesNotExistInStorage()
    {
        // Given: un token sin replay marker persistido.
        string tokenId = Guid.NewGuid().ToString("N");

        // When: el servicio valida si el token ya fue consumido.
        Result<bool> result = await _userCodeService.IsPasswordResetTokenUsedAsync(
            "missing@test.com",
            tokenId,
            CancellationToken.None);

        // Then: debe retornar false porque no hay registro.
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryFlowReturnsUnexpectedError()
    {
        // Given: un cancellation token cancelado para provocar error inesperado de repositorio.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se consulta el uso del token con la operacion cancelada.
        Result<bool> result = await _userCodeService.IsPasswordResetTokenUsedAsync(
            "cancelled@test.com",
            Guid.NewGuid().ToString("N"),
            cts.Token);

        // Then: debe propagarse el error no compensado.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }
}
