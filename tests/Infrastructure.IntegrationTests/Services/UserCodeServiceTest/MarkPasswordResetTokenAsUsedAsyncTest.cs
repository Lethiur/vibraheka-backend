using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserCodeServiceTest;

[TestFixture]
public class MarkPasswordResetTokenAsUsedAsyncTest : GenericUserCodeServiceIntegrationTest
{
    [Test]
    public async Task ShouldPersistReplayMarkerWhenTokenIsMarkedAsUsed()
    {
        // Given: email, token id and expiration values for a password reset marker
        string tokenId = Guid.NewGuid().ToString("N");
        string email = $"{tokenId}@test.com";
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // When: the service marks the token as used
        Result<MediatR.Unit> markResult = await _userCodeService.MarkPasswordResetTokenAsUsedAsync(
            email,
            tokenId,
            expiresAt,
            CancellationToken.None);

        // Then: the marker is stored and can be retrieved with the expected values
        Assert.That(markResult.IsSuccess, Is.True);

        Result<UserCodeEntity> persistedResult = await _userCodeRepository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);
        Assert.That(persistedResult.IsSuccess, Is.True);
        Assert.That(persistedResult.Value.Code, Is.EqualTo(tokenId));
        Assert.That(persistedResult.Value.UserEmail, Is.EqualTo(email));
        Assert.That(persistedResult.Value.ActionType, Is.EqualTo(ActionType.PasswordReset));
        Assert.That(persistedResult.Value.ExpiresAtUnix, Is.EqualTo(expiresAt.ToUnixTimeSeconds()));

        await CleanupTokenAsync(tokenId);
    }
}
