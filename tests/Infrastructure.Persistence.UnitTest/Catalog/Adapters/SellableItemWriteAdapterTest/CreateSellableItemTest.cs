using System.ComponentModel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemWriteAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateSellableItemTest : GenericSellableItemWriteAdapterTest
{
    [Test]
    [DisplayName("Should create transactional write with correct table, add the mapped model once, and return a non-null operation wrapper")]
    public void ShouldCreateTransactionalWriteOperationWithCorrectTableAndMappedModel()
    {
        // Given: a valid SellableItemEntity with known SellableItemID, Name and IsActive values
        SellableItemEntity product = BuildDefaultSellableItemEntity();

        // And: Some mocking
        ContextMock
            .Setup(c => c.CreateTransactWrite<SellableItemDBModel>())
            .Returns(TransactWriteMock.Object);


        // When: CreateSellableItem is called on the adapter
        ITransactionalWriteOperation result = Adapter.CreateSellableItem(product);

        // Then: AddSaveItem is called once with the model produced by the mapper (key fields must match the domain entity)
        TransactWriteMock.Verify(
            x => x.AddSaveItem(
                It.Is<SellableItemDBModel>(m =>
                    m.SellableItemID == product.SellableItemID &&
                    m.Name == product.Name &&
                    m.IsActive == product.IsActive)),
            Times.Once,
            $"Expected AddSaveItem called once with SellableItemDBModel where SellableItemID='{product.SellableItemID}' and Name='{product.Name}'");

        // Then: the returned operation is a non-null DynamoTransactionalWriteOperation wrapping the transaction
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

