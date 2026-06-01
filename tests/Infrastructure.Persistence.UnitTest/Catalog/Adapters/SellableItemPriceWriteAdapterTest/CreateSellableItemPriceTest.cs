using System.ComponentModel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.Persistence.UnitTest.Catalog.Adapters.SellableItemPriceWriteAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateSellableItemPriceTest : GenericSellableItemPriceWriteAdapterTest
{
    [Test]
    [DisplayName("Should create transactional write with correct table, add the mapped model once, and return a non-null operation wrapper")]
    public void ShouldCreateTransactionalWriteOperationWithCorrectTableAndMappedModel()
    {
        // Given: a valid SellableItemPriceEntity with known SellableItemPriceID and Kind
        SellableItemPriceEntity price = BuildDefaultSellableItemPriceEntity();

        // When: CreateSellableItemPrice is called on the adapter
        ITransactionalWriteOperation result = Adapter.CreateSellableItemPrice(price);

        // Then: AddSaveItem is called once with the model produced by the mapper (key fields must match the domain entity)
        TransactWriteMock.Verify(
            x => x.AddSaveItem(
                It.Is<SellableItemPriceDBModel>(m =>
                    m.SellableItemPriceID == price.SellableItemPriceID &&
                    m.Kind == price.Kind &&
                    m.SellableItemID == price.SellableItemID)),
            Times.Once,
            $"Expected AddSaveItem called once with SellableItemPriceDBModel where SellableItemPriceID='{price.SellableItemPriceID}' and Kind='{price.Kind}'");

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

