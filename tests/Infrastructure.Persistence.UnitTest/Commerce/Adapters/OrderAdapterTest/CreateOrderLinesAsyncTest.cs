using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Commerce.Models;
using Moq;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Commerce.Adapters.OrderAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateOrderLinesAsyncTest : GenericOrderAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success with the original order lines collection when batch write succeeds")]
    public async Task ShouldReturnSuccessWithOrderLinesWhenBatchWriteSucceeds()
    {
        // Given: a list with one valid order line and batch write mocked for success (in SetUp)
        OrderLineEntity line = BuildValidOrderLine();
        IReadOnlyCollection<OrderLineEntity> lines = [line];

        // When: CreateOrderLinesAsync is called directly on the adapter
        Result<IReadOnlyCollection<OrderLineEntity>> result =
            await Adapter.CreateOrderLinesAsync(lines, CancellationToken.None);

        // Then: result is Success and the returned collection is the original
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value, Has.Count.EqualTo(1),
            $"Expected 1 order line returned but got {result.Value.Count}");
        Assert.That(result.Value.First().OrderLineID, Is.EqualTo(line.OrderLineID),
            $"Expected OrderLineID='{line.OrderLineID}' but got '{result.Value.First().OrderLineID}'");

        ContextMock.Verify(
            x => x.CreateBatchWrite<OrderLineDBModel>(),
            Times.Once,
            "Expected CreateBatchWrite<OrderLineDBModel> called once with a valid table name");

        BatchWriteMock.Verify(
            x => x.AddPutItems(It.Is<IEnumerable<OrderLineDBModel>>(items => items != null)),
            Times.Once,
            "Expected AddPutItems called once with the mapped order line models");

        BatchWriteMock.Verify(
            x => x.ExecuteAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected ExecuteAsync called once on the batch write mock");

        ContextMock.VerifyNoOtherCalls();
        BatchWriteMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return failure with CO-002 when DynamoDB ExecuteAsync throws")]
    public async Task ShouldReturnFailureCo002WhenBatchWriteExecuteAsyncThrows()
    {
        // Given: batch write ExecuteAsync throws a general exception
        BatchWriteMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Simulated DynamoDB batch write failure"));

        OrderLineEntity line = BuildValidOrderLine();
        IReadOnlyCollection<OrderLineEntity> lines = [line];

        // When: CreateOrderLinesAsync is called on the adapter
        Result<IReadOnlyCollection<OrderLineEntity>> result =
            await Adapter.CreateOrderLinesAsync(lines, CancellationToken.None);

        // Then: result is Failure with CO-002 (mapped from GPE-999 via SaveOrderLinesAsync.MapError)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when ExecuteAsync throws but got success");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.FailedToSaveOrderLines),
            $"Expected error '{CommerceErrors.FailedToSaveOrderLines}' (CO-002) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.CreateBatchWrite<OrderLineDBModel>(),
            Times.Once,
            "Expected CreateBatchWrite<OrderLineDBModel> attempted once before ExecuteAsync failure");

        BatchWriteMock.Verify(
            x => x.AddPutItems(It.Is<IEnumerable<OrderLineDBModel>>(items => items != null)),
            Times.Once,
            "Expected AddPutItems called once on batch write mock before ExecuteAsync throws");

        BatchWriteMock.Verify(
            x => x.ExecuteAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected ExecuteAsync attempted once before throwing");

        ContextMock.VerifyNoOtherCalls();
        BatchWriteMock.VerifyNoOtherCalls();
    }
}




