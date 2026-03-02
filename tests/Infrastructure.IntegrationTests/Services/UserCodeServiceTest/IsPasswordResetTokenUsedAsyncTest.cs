using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserCodeServiceTest;

[TestFixture]
public class IsPasswordResetTokenUsedAsyncTest : GenericUserCodeServiceIntegrationTest
{
    [Test]
    public async Task ShouldReturnTrueWhenReplayMarkerExistsInStorage()
    {
        // Given: a replay marker already persisted for the requested token id
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

        // When: the service checks if this token was already used
        Result<bool> result = await _userCodeService.IsPasswordResetTokenUsedAsync(
            email,
            tokenId,
            CancellationToken.None);

        // Then: the service returns true because a marker exists
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);

        await CleanupTokenAsync(tokenId);
    }

    [Test]
    public async Task ShouldReturnFalseWhenReplayMarkerDoesNotExistInStorage()
    {
        // Given: a token id without a persisted replay marker
        string tokenId = Guid.NewGuid().ToString("N");

        // When: the service checks if the token was already used
        Result<bool> result = await _userCodeService.IsPasswordResetTokenUsedAsync(
            "missing@test.com",
            tokenId,
            CancellationToken.None);

        // Then: the service returns false because the marker is missing
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    }
}
