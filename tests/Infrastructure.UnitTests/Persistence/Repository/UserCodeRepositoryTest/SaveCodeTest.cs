using Amazon.DynamoDBv2.DataModel;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserCodeRepositoryTest;

[TestFixture]
public class SaveCodeTest : GenericUserCodeRepositoryTest
{
    [Test]
    public async Task ShouldPersistMappedModelWithExpectedFieldsWhenSaveSucceeds()
    {
        // Given: a valid domain entity to be persisted
        AWSXRayRecorder.Instance.TraceContext.SetEntity(new Segment("mock"));
        UserCodeEntity entity = new()
        {
            UserEmail = "user@test.com",
            ActionType = ActionType.PasswordReset,
            Code = "token-123",
            ExpiresAtUnix = 9999999999,
            CreatedBy = "user@test.com",
            LastModifiedBy = "user@test.com"
        };

        ContextMock.Setup(x => x.SaveAsync(
                It.IsAny<UserCodeDBModel>(),
                It.IsAny<SaveConfig>(),
                CancellationToken.None))
            .Returns(Task.CompletedTask);

        // When: saving the entity through the repository
        Result<Unit> result = await Repository.SaveCode(entity, CancellationToken.None);

        // Then: operation succeeds and mapped model is saved with expected values
        Assert.That(result.IsSuccess, Is.True);
        ContextMock.Verify(x => x.SaveAsync(
            It.Is<UserCodeDBModel>(model =>
                model.UserEmail == entity.UserEmail &&
                model.ActionType == entity.ActionType &&
                model.Code == entity.Code &&
                model.ExpiresAtUnix == entity.ExpiresAtUnix),
            It.Is<SaveConfig>(config => config.OverrideTableName == ConfigMock.UserCodesTable),
            CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ShouldReturnFailureWhenContextThrowsOnSave()
    {
        // Given: a persistence exception from DynamoDB context
        UserCodeEntity entity = new()
        {
            UserEmail = "user@test.com",
            ActionType = ActionType.PasswordReset,
            Code = "token-123"
        };
        ContextMock.Setup(x => x.SaveAsync(
                It.IsAny<UserCodeDBModel>(),
                It.IsAny<SaveConfig>(),
                CancellationToken.None))
            .ThrowsAsync(new Exception("Save failed"));

        // When: saving the entity through the repository
        Result<Unit> result = await Repository.SaveCode(entity, CancellationToken.None);

        // Then: repository returns mapped generic persistence failure
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError));
    }
}
