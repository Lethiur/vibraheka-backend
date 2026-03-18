using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserCodeServiceTest;

[TestFixture]
public class MarkPasswordResetTokenAsUsedAsyncTest : GenericUserCodeServiceIntegrationTest
{
    [Test]
    public async Task ShouldPersistReplayMarkerWhenTokenIsMarkedAsUsed()
    {
        // Given: email, token id y expiracion validos para marcar token usado.
        string tokenId = Guid.NewGuid().ToString("N");
        string email = $"{tokenId}@test.com";
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // When: el servicio persiste el marcador de token consumido.
        Result<Unit> markResult = await _userCodeService.MarkPasswordResetTokenAsUsedAsync(
            email,
            tokenId,
            expiresAt,
            CancellationToken.None);

        // Then: debe guardarse y poder recuperarse con los mismos datos.
        Assert.That(markResult.IsSuccess, Is.True);

        Result<UserCodeEntity> persistedResult = await _userCodeRepository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);
        Assert.That(persistedResult.IsSuccess, Is.True);
        Assert.That(persistedResult.Value.Code, Is.EqualTo(tokenId));
        Assert.That(persistedResult.Value.UserEmail, Is.EqualTo(email));
        Assert.That(persistedResult.Value.ActionType, Is.EqualTo(ActionType.PasswordReset));
        Assert.That(persistedResult.Value.ExpiresAtUnix, Is.EqualTo(expiresAt.ToUnixTimeSeconds()));

        await CleanupTokenAsync(tokenId);
    }

    [Test]
    public async Task ShouldReturnFailureWhenRepositoryCannotPersistReplayMarker()
    {
        // Given: un cancellation token cancelado para forzar fallo en persistencia.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se intenta marcar el token como usado con la operacion cancelada.
        Result<Unit> markResult = await _userCodeService.MarkPasswordResetTokenAsUsedAsync(
            "cancelled@test.com",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow.AddMinutes(30),
            cts.Token);

        // Then: debe retornar error de persistencia.
        Assert.That(markResult.IsFailure, Is.True);
        Assert.That(markResult.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
