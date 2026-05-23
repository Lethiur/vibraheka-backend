using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.RecordingRepositoryTest;

[TestFixture]
public sealed class GetAllAsyncTest : GenericRecordingRepositoryTest
{

    [Test]
    [DisplayName("Should return all recordings mapped to domain entities when DynamoDB scan succeeds")]
    public async Task ShouldReturnAllRecordingsMappedToDomainWhenScanSucceeds()
    {
        // Given: DynamoDB scan returns two recording models
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<RecordingDBModel> models =
        [
            new RecordingDBModel
            {
                Id = "id-1",
                Name = "Meditacion",
                Description = "Desc1",
                Type = RecordingType.Meditacion,
                Created = now
            },
            new RecordingDBModel
            {
                Id = "id-2",
                Name = "Taller",
                Description = "Desc2",
                Type = RecordingType.Taller,
                Created = now
            }
        ];

        Mock<IAsyncSearch<RecordingDBModel>> searchMock = new();
        searchMock
            .Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(models);

        ContextMock
            .Setup(c => c.ScanAsync<RecordingDBModel>(
                It.IsAny<IEnumerable<ScanCondition>>()))
            .Returns(searchMock.Object);

        // When: GetAllAsync is called
        Result<IEnumerable<RecordingEntity>> result = await Repository.GetAllAsync(CancellationToken.None);

        // Then: result is success with two correctly mapped entities
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        List<RecordingEntity> entities = result.Value.ToList();
        Assert.That(entities, Has.Count.EqualTo(2),
            $"Expected 2 entities but got {entities.Count}");
        Assert.That(entities[0].Id, Is.EqualTo("id-1"),
            $"Expected first entity Id 'id-1' but got '{entities[0].Id}'");
        Assert.That(entities[1].Id, Is.EqualTo("id-2"),
            $"Expected second entity Id 'id-2' but got '{entities[1].Id}'");

        ContextMock.Verify(
            c => c.ScanAsync<RecordingDBModel>(
                It.IsAny<IEnumerable<ScanCondition>>()),
            Times.Once,
            "Expected ScanAsync to be called exactly once with correct table name");
    }

    [Test]
    [DisplayName("Should return empty collection when DynamoDB scan returns no items")]
    public async Task ShouldReturnEmptyCollectionWhenScanReturnsNoItems()
    {
        // Given: DynamoDB scan returns an empty list
        Mock<IAsyncSearch<RecordingDBModel>> searchMock = new();
        searchMock
            .Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ContextMock
            .Setup(c => c.ScanAsync<RecordingDBModel>(
                It.IsAny<IEnumerable<ScanCondition>>()))
            .Returns(searchMock.Object);

        // When: GetAllAsync is called
        Result<IEnumerable<RecordingEntity>> result = await Repository.GetAllAsync(CancellationToken.None);

        // Then: result is success with an empty collection
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.Count(), Is.EqualTo(0),
            $"Expected 0 entities but got {result.Value.Count()}");

        ContextMock.Verify(
            c => c.ScanAsync<RecordingDBModel>(
                It.IsAny<IEnumerable<ScanCondition>>()),
            Times.Once,
            "Expected ScanAsync to be called exactly once");

    }

    [Test]
    [DisplayName("Should return failure with GeneralError when DynamoDB scan throws an unexpected exception")]
    public async Task ShouldReturnFailureWhenScanThrowsUnexpectedException()
    {
        // Given: DynamoDB context throws an unexpected exception during scan
        ContextMock
            .Setup(c => c.ScanAsync<RecordingDBModel>(
                It.IsAny<IEnumerable<ScanCondition>>(),
                It.IsAny<ScanConfig>()))
            .Throws(new Exception("Unexpected DynamoDB error"));

        // When: GetAllAsync is called
        Result<IEnumerable<RecordingEntity>> result = await Repository.GetAllAsync(CancellationToken.None);

        // Then: result is failure with the general error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with count: '{(result.IsSuccess ? result.Value.Count() : 0)}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.GeneralError),
            $"Expected error '{GenericPersistenceErrors.GeneralError}' but got '{result.Error}'");

        ContextMock.Verify(
            c => c.ScanAsync<RecordingDBModel>(
                It.IsAny<IEnumerable<ScanCondition>>()),
            Times.Once,
            "Expected ScanAsync to be called exactly once before throwing");

        ContextMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return failure with ProvisionedThroughputExceeded when DynamoDB throttles the scan")]
    public async Task ShouldReturnFailureWhenScanThrowsProvisionedThroughputExceededException()
    {
        // Given: DynamoDB throws ProvisionedThroughputExceededException during scan
        ContextMock
            .Setup(c => c.ScanAsync<RecordingDBModel>(It.IsAny<IEnumerable<ScanCondition>>()))
            .Throws(new ProvisionedThroughputExceededException("Throughput exceeded"));

        // When: GetAllAsync is called
        Result<IEnumerable<RecordingEntity>> result = await Repository.GetAllAsync(CancellationToken.None);

        // Then: result is failure with the throughput-exceeded error code
        Assert.That(result.IsFailure, Is.True,
            $"Expected failure but got success with count: '{(result.IsSuccess ? result.Value.Count() : 0)}'");
        Assert.That(result.Error, Is.EqualTo(GenericPersistenceErrors.ProvisionedThroughputExceeded),
            $"Expected error '{GenericPersistenceErrors.ProvisionedThroughputExceeded}' but got '{result.Error}'");
    }
}
