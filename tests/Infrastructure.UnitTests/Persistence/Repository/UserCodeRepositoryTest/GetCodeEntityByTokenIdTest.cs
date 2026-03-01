using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserCodeRepositoryTest;

[TestFixture]
public class GetCodeEntityByTokenIdTest : GenericUserCodeRepositoryTest
{
    [Test]
    public async Task ShouldReturnMappedDomainEntityWhenRecordExists()
    {
        // Given: a matching record in DynamoDB
        const string tokenId = "token-123";
        UserCodeDBModel model = new()
        {
            Code = tokenId,
            UserEmail = "user@test.com",
            ActionType = ActionType.PasswordReset,
            ExpiresAtUnix = 9999999999
        };
        ContextMock.Setup(x => x.LoadAsync<UserCodeDBModel>(tokenId, It.IsAny<LoadConfig>(), CancellationToken.None))
            .ReturnsAsync(model);

        // When: retrieving the token marker by token id
        Result<UserCodeEntity> result = await Repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: repository returns mapped domain entity
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Code, Is.EqualTo(model.Code));
        Assert.That(result.Value.UserEmail, Is.EqualTo(model.UserEmail));
        Assert.That(result.Value.ActionType, Is.EqualTo(model.ActionType));
        Assert.That(result.Value.ExpiresAtUnix, Is.EqualTo(model.ExpiresAtUnix));

        ContextMock.Verify(x => x.LoadAsync<UserCodeDBModel>(
            tokenId,
            It.Is<LoadConfig>(config => config.OverrideTableName == ConfigMock.UserCodesTable),
            CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldMapNoRecordsFoundToUserCodeNoRecordFound()
    {
        // Given: no record for the provided token id
        const string tokenId = "token-123";
        ContextMock.Setup(x => x.LoadAsync<UserCodeDBModel>(tokenId, It.IsAny<LoadConfig>(), CancellationToken.None))
            .ReturnsAsync((UserCodeDBModel)null!);

        // When: retrieving the token marker by token id
        Result<UserCodeEntity> result = await Repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: repository maps no-record error to domain-specific error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserCodeErrors.NoRecordFound));
    }

    [Test]
    public async Task ShouldMapUnexpectedPersistenceErrorToGenericAppError()
    {
        // Given: a generic persistence error while loading by token id
        const string tokenId = "token-123";
        ContextMock.Setup(x => x.LoadAsync<UserCodeDBModel>(tokenId, It.IsAny<LoadConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Unexpected load failure"));

        // When: retrieving the token marker by token id
        Result<UserCodeEntity> result = await Repository.GetCodeEntityByTokenId(tokenId, CancellationToken.None);

        // Then: repository maps to generic app error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AppErrors.GenericError));
        Assert.That(result.Error, Is.Not.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
