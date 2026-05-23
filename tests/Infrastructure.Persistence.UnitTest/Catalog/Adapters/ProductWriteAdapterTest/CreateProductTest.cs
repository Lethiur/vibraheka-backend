using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.ProductWriteAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateProductTest : GenericProductWriteAdapterTest
{
    [Test]
    [DisplayName("Should create transactional write with correct table, add the mapped model once, and return a non-null operation wrapper")]
    public void ShouldCreateTransactionalWriteOperationWithCorrectTableAndMappedModel()
    {
        // Given: a valid ProductEntity with known Name and Description
        ProductEntity product = BuildDefaultProductEntity();
        
        // And: Some mocking
        Mock<ITransactWrite<ProductDBModel>> transactMock = new Mock<ITransactWrite<ProductDBModel>>();
        ContextMock
            .Setup(c => c.CreateTransactWrite<ProductDBModel>())
            .Returns(transactMock.Object);

        // When: CreateProduct is called on the adapter
        ITransactionalWriteOperation result = Adapter.CreateProduct(product);

        // Then: AddSaveItem is called once with the model produced by the mapper (Name and Description must match the domain entity)
        transactMock.Verify(
            x => x.AddSaveItem(
                It.Is<ProductDBModel>(m =>
                    m.Name == product.Name &&
                    m.Description == product.Description)),
            Times.Once,
            $"Expected AddSaveItem called once with a ProductDBModel where Name='{product.Name}' and Description='{product.Description}'");

        // Then: the returned operation is a non-null DynamoTransactionalWriteOperation wrapping the transaction
        Assert.That(result, Is.Not.Null,
            "Expected a non-null ITransactionalWriteOperation");
        Assert.That(result, Is.InstanceOf<DynamoTransactionalWriteOperation>(),
            $"Expected DynamoTransactionalWriteOperation but got '{result.GetType().Name}'");

        ContextMock.VerifyNoOtherCalls();
        TransactWriteMock.VerifyNoOtherCalls();
    }
}

