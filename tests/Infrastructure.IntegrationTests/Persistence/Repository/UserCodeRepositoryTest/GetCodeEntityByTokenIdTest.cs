using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
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
        // Given: un token marker existente en DynamoDB.
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

        // When: se busca el marker por token id.
        Result<UserCodeEntity> result = await _repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: debe devolverse la entidad correctamente mapeada.
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
        // Given: un token id inexistente.
        string tokenId = Guid.NewGuid().ToString("N");

        // When: se busca un marker inexistente.
        Result<UserCodeEntity> result = await _repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: debe devolverse error NoRecordFound.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserCodeErrors.NoRecordFound));
    }

    [Test]
    public async Task ShouldMapUnexpectedPersistenceFailureToGenericAppError()
    {
        // Given: un cancellation token cancelado para forzar error interno de persistencia.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        // When: se busca el token con la operacion cancelada.
        Result<UserCodeEntity> result = await _repository.GetCodeEntityByTokenId(Guid.NewGuid().ToString("N"), cts.Token);

        // Then: debe mapearse al error generico de aplicacion.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
    }
}
