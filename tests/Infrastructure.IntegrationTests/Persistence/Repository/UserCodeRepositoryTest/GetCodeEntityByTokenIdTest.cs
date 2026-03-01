using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserCodeRepositoryTest;

[TestFixture]
public class GetCodeEntityByTokenIdTest : GenericUserCodeRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldReturnUserCodeEntityWhenTokenExists()
    {
        // Given: an existing token marker already stored in DynamoDB
        string tokenId = Guid.NewGuid().ToString("N");
        UserCodeEntity seededEntity = new()
        {
            UserEmail = $"{tokenId}@test.com",
            ActionType = ActionType.PasswordReset,
            Code = tokenId,
            ExpiresAtUnix = DateTimeOffset.UtcNow.AddMinutes(25).ToUnixTimeSeconds(),
            CreatedBy = "integration-test",
            LastModifiedBy = "integration-test"
        };

        await _repository.SaveCode(seededEntity, CancellationToken.None);

        // When: requesting the marker by its token id
        Result<UserCodeEntity> result = await _repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: the repository returns the mapped domain entity
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Code, Is.EqualTo(seededEntity.Code));
        Assert.That(result.Value.UserEmail, Is.EqualTo(seededEntity.UserEmail));
        Assert.That(result.Value.ActionType, Is.EqualTo(seededEntity.ActionType));
        Assert.That(result.Value.ExpiresAtUnix, Is.EqualTo(seededEntity.ExpiresAtUnix));

        await CleanupTokenAsync(tokenId);
    }

    [Test]
    public async Task ShouldReturnNoRecordFoundWhenTokenDoesNotExist()
    {
        // Given: a token id that does not exist in the UserCodes table
        string tokenId = Guid.NewGuid().ToString("N");

        // When: requesting the marker by that token id
        Result<UserCodeEntity> result = await _repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: the repository reports that no matching marker exists
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserCodeErrors.NoRecordFound));
    }
}
