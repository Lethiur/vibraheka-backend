using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.ProductWriteAdapterTest;

public abstract class GenericProductWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<ProductDBModel>> TransactWriteMock = default!;
    protected ProductEntityMapper Mapper = default!;
    protected ProductWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<ProductDBModel>>();
        Mapper = new ProductEntityMapper();
        Adapter = new ProductWriteAdapter(Mapper, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<ProductDBModel>(It.IsAny<TransactWriteConfig>()))
            .Returns(TransactWriteMock.Object);
    }

    protected static ProductEntity BuildDefaultProductEntity() =>
        new()
        {
            ProductID = "prod-unit-test-001",
            Name = "Test Product Adapter",
            Description = "Test Product Description Adapter",
        };
}

