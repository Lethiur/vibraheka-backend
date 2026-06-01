using System.ComponentModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Common.Errors;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.DynamoAtomicWriteStoreTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CommitAsyncTest : GenericDynamoAtomicWriteStoreTest
{
    #region Happy Path

    [Test]
    [DisplayName("Should return success when batch is empty")]
    public async Task ShouldReturnSuccessWhenBatchIsEmpty()
    {
        // Given: an empty batch
        TransactionalWriteBatch batch = new("idem-empty-batch");

        ContextMock
            .Setup(x => x.ExecuteTransactWriteAsync(It.IsAny<ITransactWrite[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When: CommitAsync is called with the empty batch
        Result<Unit> result = await Store.CommitAsync(batch, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success for empty batch but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
    }

    [Test]
    [DisplayName("Should return success when batch has operations and DynamoDB ExecuteTransactWriteAsync succeeds")]
    public async Task ShouldReturnSuccessWhenBatchOperationsCommitSuccessfully()
    {
        // Given: a batch with 3 valid operations
        TransactionalWriteBatch batch = BuildBatchWithOperations(3, "idem-success-key");

        ContextMock
            .Setup(x => x.ExecuteTransactWriteAsync(It.IsAny<ITransactWrite[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // When: CommitAsync is called
        Result<Unit> result = await Store.CommitAsync(batch, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");

        ContextMock.Verify(
            x => x.ExecuteTransactWriteAsync(
                It.Is<ITransactWrite[]>(items => items.Length == 3),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected ExecuteTransactWriteAsync called once with 3 operations");
    }

    #endregion

    #region Failure — Batch Too Large

    [Test]
    [DisplayName("Should return failure with E-999 when batch has more than 100 operations")]
    public async Task ShouldReturnFailureWithE999WhenBatchExceeds100Operations()
    {
        // Given: a batch with 101 operations (exceeds DynamoDB limit)
        TransactionalWriteBatch batch = BuildBatchWithOperations(101, "idem-overflow-key");

        // When: CommitAsync is called
        Result<Unit> result = await Store.CommitAsync(batch, CancellationToken.None);

        // Then: result is failure with E-999
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when batch exceeds 100 operations but got success");
        Assert.That(result.Error, Is.EqualTo(DomainErrors.GenericError),
            $"Expected '{DomainErrors.GenericError}' but got '{result.Error}'");

        ContextMock.Verify(
            x => x.ExecuteTransactWriteAsync(It.IsAny<ITransactWrite[]>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Expected ExecuteTransactWriteAsync never called when batch exceeds limit");
    }

    #endregion

    #region Failure — AmazonDynamoDBException

    [Test]
    [DisplayName("Should return failure with E-999 when AmazonDynamoDBException is thrown")]
    public async Task ShouldReturnFailureWithE999WhenAmazonDynamoDBExceptionIsThrown()
    {
        // Given: a batch with 1 operation and DynamoDB throws AmazonDynamoDBException
        TransactionalWriteBatch batch = BuildBatchWithOperations(1, "idem-db-exception-key");

        ContextMock
            .Setup(x => x.ExecuteTransactWriteAsync(It.IsAny<ITransactWrite[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonDynamoDBException("Simulated DynamoDB error"));

        // When: CommitAsync is called
        Result<Unit> result = await Store.CommitAsync(batch, CancellationToken.None);

        // Then: result is failure with E-999
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when AmazonDynamoDBException is thrown but got success");
        Assert.That(result.Error, Is.EqualTo(DomainErrors.GenericError),
            $"Expected '{DomainErrors.GenericError}' but got '{result.Error}'");

        ContextMock.Verify(
            x => x.ExecuteTransactWriteAsync(It.IsAny<ITransactWrite[]>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Expected ExecuteTransactWriteAsync called once before throwing exception");
    }

    #endregion
}

