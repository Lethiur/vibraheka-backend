using System.ComponentModel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Commerce.Models;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Commerce.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Commerce.Adapters.OrderWriteAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateOrderTest : GenericOrderWriteAdapterTest
{
    [Test]
    [DisplayName("Should create transactional write with correct table, add the mapped model once, and return a non-null operation wrapper")]
    public void ShouldCreateTransactionalWriteOperationWithCorrectTableAndMappedModel()
    {
        // Given: a valid OrderEntity with known OrderID and UserId
        OrderEntity order = BuildDefaultOrderEntity();

        // When: CreateOrder is called on the adapter
        ITransactionalWriteOperation result = Adapter.CreateOrder(order);
        
        // Then: AddSaveItem is called once with the model produced by the mapper
        TransactWriteMock.Verify(
            x => x.AddSaveItem(
                It.Is<OrderDBModel>(m =>
                    m.OrderID == order.OrderID &&
                    m.UserID == order.UserID)),
            Times.Once,
            $"Expected AddSaveItem called once with OrderDBModel where OrderID='{order.OrderID}' and UserId='{order.UserID}'");

        // Then: the returned operation is a non-null DynamoTransactionalWriteOperation
        Assert.That(result, Is.Not.Null,
            "Expected a non-null ITransactionalWriteOperation");
        Assert.That(result, Is.InstanceOf<DynamoTransactionalWriteOperation>(),
            $"Expected DynamoTransactionalWriteOperation but got '{result.GetType().Name}'");

        DynamoTransactionalWriteOperation dynamoOperation = (DynamoTransactionalWriteOperation)result;
        Assert.That(dynamoOperation.Item, Is.SameAs(TransactWriteMock.Object),
            "Expected the wrapped ITransactWrite item to be the mock returned by CreateTransactWrite");

        ContextMock.VerifyNoOtherCalls();
        TransactWriteMock.VerifyNoOtherCalls();
    }
}

