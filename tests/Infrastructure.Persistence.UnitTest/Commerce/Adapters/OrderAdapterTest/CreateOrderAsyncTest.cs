using System.ComponentModel;
using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Commerce.Models;
using Moq;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Errors;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Commerce.Adapters.OrderAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateOrderAsyncTest : GenericOrderAdapterTest
{
    [Test]
    [DisplayName("Should return Result.Success with the order entity when DynamoDB calls succeed")]
    public async Task ShouldReturnSuccessWithOrderEntityWhenAllContextCallsSucceed()
    {
        // Given: a valid order entity with no lines and all mocks configured for happy path (in SetUp)
        OrderEntity order = BuildValidOrderEntityNoLines();

        // When: CreateOrderAsync is called on the adapter
        Result<OrderEntity> result = await Adapter.CreateOrderAsync(order, CancellationToken.None);

        // Then: result is Success and contains the original entity
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsSuccess ? "N/A" : result.Error)}'");
        Assert.That(result.Value.OrderID, Is.EqualTo(order.OrderID),
            $"Expected returned OrderID='{order.OrderID}' but got '{result.Value.OrderID}'");

        ContextMock.Verify(
            x => x.SaveAsync(
                It.Is<OrderDBModel>(m => m.OrderID == order.OrderID && m.UserID == order.UserID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            $"Expected SaveAsync called once with OrderDBModel for OrderID='{order.OrderID}'");

        ContextMock.Verify(
            x => x.CreateBatchWrite<OrderLineDBModel>(),
            Times.Once,
            "Expected CreateBatchWrite<OrderLineDBModel> called once for empty lines batch");

        BatchWriteMock.Verify(
            x => x.AddPutItems(It.Is<IEnumerable<OrderLineDBModel>>(items => items != null)),
            Times.Once,
            "Expected AddPutItems called once on the batch write mock");

        BatchWriteMock.Verify(
            x => x.ExecuteAsync(It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected ExecuteAsync called once on the batch write mock");

        ContextMock.VerifyNoOtherCalls();
        BatchWriteMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return failure with CO-001 when DynamoDB SaveAsync throws for the order")]
    public async Task ShouldReturnFailureCo001WhenContextSaveOrderThrows()
    {
        // Given: SaveAsync throws a general exception simulating a DynamoDB outage
        ContextMock
            .Setup(x => x.SaveAsync(It.IsAny<OrderDBModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Simulated DynamoDB failure"));

        OrderEntity order = BuildValidOrderEntityNoLines();

        // When: CreateOrderAsync is called on the adapter
        Result<OrderEntity> result = await Adapter.CreateOrderAsync(order, CancellationToken.None);

        // Then: result is Failure with CO-001 (mapped from GPE-999 via SaveOrderAsync.MapError)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when SaveAsync throws but got success");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.FailedToSaveOrder),
            $"Expected error '{CommerceErrors.FailedToSaveOrder}' (CO-001) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.SaveAsync(
                It.Is<OrderDBModel>(m => m.OrderID == order.OrderID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync attempted once before failing");

        ContextMock.VerifyNoOtherCalls();
        BatchWriteMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should return failure with CO-002 when DynamoDB ExecuteAsync throws for the order lines")]
    public async Task ShouldReturnFailureCo002WhenBatchWriteExecuteAsyncThrows()
    {
        // Given: SaveAsync succeeds but ExecuteAsync throws simulating a DynamoDB batch write failure
        OrderEntity order = BuildValidOrderEntityNoLines();
        order.Lines.Add(BuildValidOrderLine());

        BatchWriteMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Simulated DynamoDB batch write failure"));

        // When: CreateOrderAsync is called on the adapter
        Result<OrderEntity> result = await Adapter.CreateOrderAsync(order, CancellationToken.None);

        // Then: result is Failure with CO-002 (mapped from GPE-999 via SaveOrderLinesAsync.MapError)
        Assert.That(result.IsFailure, Is.True,
            "Expected failure when ExecuteAsync throws but got success");
        Assert.That(result.Error, Is.EqualTo(CommerceErrors.FailedToSaveOrderLines),
            $"Expected error '{CommerceErrors.FailedToSaveOrderLines}' (CO-002) but got '{result.Error}'");

        ContextMock.Verify(
            x => x.SaveAsync(
                It.Is<OrderDBModel>(m => m.OrderID == order.OrderID),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)),
            Times.Once,
            "Expected SaveAsync called once before batch write failure");

        ContextMock.Verify(
            x => x.CreateBatchWrite<OrderLineDBModel>(),
            Times.Once,
            "Expected CreateBatchWrite<OrderLineDBModel> called once before ExecuteAsync failure");

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




