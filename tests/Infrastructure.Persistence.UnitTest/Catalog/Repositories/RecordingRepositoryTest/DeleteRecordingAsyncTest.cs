using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Repositories.RecordingRepositoryTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class DeleteRecordingAsyncTest : GenericRecordingRepositoryTest
{
    private static RecordingEntity CreateValidRecordingEntity(string? id = null) =>
        new()
        {
            RecordingID = id ?? Guid.NewGuid().ToString(),
            Name = "Sesion de meditacion",
            Description = "Descripcion de la sesion de meditacion guiada",
            RecordingType = RecordingType.Meditacion,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "admin-user-id",
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin-user-id",
        };

    [Test]
    [DisplayName("Should map RecordingEntity to RecordingDBModel and call DeleteAsync with correct OverrideTableName")]
    public async Task ShouldMapEntityToModelAndCallDeleteAsyncWithCorrectTableName()
    {
        // Given: a valid RecordingEntity and DynamoDB context that deletes successfully
        RecordingEntity entity = CreateValidRecordingEntity();

        ContextMock
            .Setup(c => c.DeleteAsync(
                It.IsAny<RecordingDBModel>(),
                It.IsAny<DeleteConfig>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When: DeleteRecordingAsync is called
        Result result = await Repository.DeleteRecordingAsync(entity, CancellationToken.None);

        // Then: result should be success and DeleteAsync should have been called with the mapped model
        Assert.That(
            result.IsSuccess,
            Is.True,
            $"Expected result to be success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        ContextMock.Verify(
            c => c.DeleteAsync(
                It.Is<RecordingDBModel>(m =>
                    m.Id == entity.RecordingID &&
                    m.Name == entity.Name),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected DeleteAsync called once with model mapped from entity.Id={entity.RecordingID}");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return Result.Success when DynamoDB DeleteAsync completes without exception")]
    public async Task ShouldReturnSuccessWhenDynamoDbDeleteAsyncCompletesWithoutException()
    {
        // Given: a valid RecordingEntity and a DynamoDB context that completes successfully
        RecordingEntity entity = CreateValidRecordingEntity("existing-recording-id");

        ContextMock
            .Setup(c => c.DeleteAsync(
                It.IsAny<RecordingDBModel>(),
                It.IsAny<DeleteConfig>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When: DeleteRecordingAsync is called
        Result result = await Repository.DeleteRecordingAsync(entity, CancellationToken.None);

        // Then: result should be success
        Assert.That(
            result.IsSuccess,
            Is.True,
            $"Expected Result.Success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        ContextMock.Verify(
            c => c.DeleteAsync(
                It.Is<RecordingDBModel>(m => m.Id == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteAsync to be called exactly once with the correct entity id and table name");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return GPE-999 failure when DynamoDB DeleteAsync throws an unexpected exception")]
    public async Task ShouldReturnGPE999FailureWhenDynamoDbDeleteAsyncThrowsUnexpectedException()
    {
        // Given: a valid RecordingEntity and DynamoDB context that throws an unexpected exception
        RecordingEntity entity = CreateValidRecordingEntity();
        InvalidOperationException expectedException = new("DynamoDB connection failed");

        ContextMock
            .Setup(c => c.DeleteAsync(
                It.IsAny<RecordingDBModel>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // When: DeleteRecordingAsync is called
        Result result = await Repository.DeleteRecordingAsync(entity, CancellationToken.None);

        // Then: result should be failure with GPE-999 (general error)
        Assert.That(
            result.IsFailure,
            Is.True,
            "Expected Result.Failure when DeleteAsync throws an unexpected exception");

        Assert.That(
            result.Error,
            Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected GPE-999 GeneralError but got: '{result.Error}'");

        ContextMock.Verify(
            c => c.DeleteAsync(
                It.Is<RecordingDBModel>(m => m.Id == entity.RecordingID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected DeleteAsync to be called once before throwing");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return ProvisionedThroughputExceeded failure when DynamoDB throttles the delete")]
    public async Task ShouldReturnProvisionedThroughputExceededFailureWhenDynamoDbThrottlesDelete()
    {
        // Given: a valid RecordingEntity and DynamoDB context that throws ProvisionedThroughputExceededException
        RecordingEntity entity = CreateValidRecordingEntity();

        ContextMock
            .Setup(c => c.DeleteAsync(
                It.IsAny<RecordingDBModel>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProvisionedThroughputExceededException("Throughput exceeded"));

        // When: DeleteRecordingAsync is called
        Result result = await Repository.DeleteRecordingAsync(entity, CancellationToken.None);

        // Then: result should be failure with ProvisionedThroughputExceeded error code
        Assert.That(
            result.IsFailure,
            Is.True,
            "Expected failure when DynamoDB throttles the delete request");

        Assert.That(
            result.Error,
            Is.EqualTo(GenericPersistenceErrors.ProvisionedThroughputExceeded),
            $"Expected ProvisionedThroughputExceeded error but got: '{result.Error}'");
    }
}

