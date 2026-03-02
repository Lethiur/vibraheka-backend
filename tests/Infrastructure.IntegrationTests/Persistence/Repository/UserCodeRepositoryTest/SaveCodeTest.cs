using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserCodeRepositoryTest;

[TestFixture]
public class SaveCodeTest : GenericUserCodeRepositoryIntegrationTest
{
    [Test]
    public async Task ShouldPersistUserCodeRecordWhenSavingEntity()
    {
        // Given: a valid user code entity ready to be stored in the UserCodes table
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

        // When: the repository persists the entity
        Result<Unit> result = await _repository.SaveCode(entity, CancellationToken.None);

        // Then: the save succeeds and the persisted record matches the original payload
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
}
