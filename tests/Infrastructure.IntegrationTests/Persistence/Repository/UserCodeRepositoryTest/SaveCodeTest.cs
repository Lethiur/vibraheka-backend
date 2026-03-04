using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserCodeRepositoryTest;

[TestFixture]
public class SaveCodeTest : GenericUserCodeRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldPersistUserCodeRecordWhenSavingEntity()
    {
        // Given: una entidad de codigo valida para persistir.
        string tokenId = Guid.NewGuid().ToString("N");
        UserCodeEntity entity = new()
        {
            UserEmail = $"{tokenId}@test.com",
            ActionType = ActionType.PasswordReset,
            Code = tokenId,
            ExpiresAtUnix = DateTimeOffset.UtcNow.AddMinutes(20).ToUnixTimeSeconds(),
            CreatedBy = "integration-test",
            LastModifiedBy = "integration-test"
        };

        // When: se guarda la entidad en el repositorio.
        Result<Unit> result = await _repository.SaveCode(entity, CancellationToken.None);

        // Then: debe persistirse con los mismos datos en DynamoDB.
        Assert.That(result.IsSuccess, Is.True);

        UserCodeDBModel? persistedModel = await _dynamoDbContext.LoadAsync<UserCodeDBModel>(
            tokenId,
            new LoadConfig
            {
                OverrideTableName = _configuration.UserCodesTable
            },
            CancellationToken.None);

        Assert.That(persistedModel, Is.Not.Null);
        Assert.That(persistedModel!.Code, Is.EqualTo(entity.Code));
        Assert.That(persistedModel.UserEmail, Is.EqualTo(entity.UserEmail));
        Assert.That(persistedModel.ActionType, Is.EqualTo(entity.ActionType));
        Assert.That(persistedModel.ExpiresAtUnix, Is.EqualTo(entity.ExpiresAtUnix));

        await CleanupTokenAsync(tokenId);
    }

    [Test]
    public async Task ShouldReturnFailureWhenSaveOperationIsCancelled()
    {
        // Given: un cancellation token cancelado para provocar fallo de guardado.
        using CancellationTokenSource cts = new();
        cts.Cancel();

        UserCodeEntity entity = new()
        {
            UserEmail = "cancelled@test.com",
            ActionType = ActionType.PasswordReset,
            Code = Guid.NewGuid().ToString("N"),
            ExpiresAtUnix = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
        };

        // When: se intenta persistir con la operacion cancelada.
        Result<Unit> result = await _repository.SaveCode(entity, cts.Token);

        // Then: debe retornar error general de persistencia.
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
