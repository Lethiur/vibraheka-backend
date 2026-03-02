using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using Moq;
namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.ActionLogRepositoryTest;

[TestFixture]
public class GetActionLogForUserTest : GenericActionLogRepositoryTest
{
    [Test]
    public async Task ShouldReturnMappedActionLogWhenRecordExists()
    {
        // Given
        ActionLogDBModel dbModel = new()
        {
            ID = "user-1",
            Action = ActionType.UserVerification,
            Timestamp = DateTimeOffset.UtcNow
        };

        ContextMock.Setup(x => x.LoadAsync<ActionLogDBModel>("user-1", ActionType.UserVerification, It.IsAny<LoadConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbModel);

        // When
        Result<ActionLogEntity> result = await Repository.GetActionLogForUser("user-1", ActionType.UserVerification, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ID, Is.EqualTo("user-1"));
        Assert.That(result.Value.Action, Is.EqualTo(ActionType.UserVerification));
    }

    [Test]
    public async Task ShouldReturnActionLogNotFoundWhenNoRecordExists()
    {
        // Given
        ContextMock.Setup(x => x.LoadAsync<ActionLogDBModel>("user-1", ActionType.UserVerification, It.IsAny<LoadConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionLogDBModel)null!);

        // When
        Result<ActionLogEntity> result = await Repository.GetActionLogForUser("user-1", ActionType.UserVerification, CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(ActionLogErrors.ActionLogNotFound));
    }
}
