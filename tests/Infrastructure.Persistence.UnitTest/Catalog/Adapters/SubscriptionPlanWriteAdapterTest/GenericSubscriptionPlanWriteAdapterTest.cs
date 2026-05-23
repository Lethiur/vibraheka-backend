using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SubscriptionPlanWriteAdapterTest;

public abstract class GenericSubscriptionPlanWriteAdapterTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ITransactWrite<SubscriptionPlanDBModel>> TransactWriteMock = default!;
    protected SubscriptionPlanEntityMapper Mapper = default!;
    protected SubscriptionPlanWriteAdapter Adapter = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        TransactWriteMock = new Mock<ITransactWrite<SubscriptionPlanDBModel>>();
        
        Mapper = new SubscriptionPlanEntityMapper();
        Adapter = new SubscriptionPlanWriteAdapter(Mapper, ContextMock.Object);

        ContextMock
            .Setup(x => x.CreateTransactWrite<SubscriptionPlanDBModel>(It.IsAny<TransactWriteConfig>()))
            .Returns(TransactWriteMock.Object);
    }

    protected static SubscriptionPlanEntity BuildDefaultSubscriptionPlanEntity()
    {
        SubscriptionPlanEntity entity = new();
        typeof(SubscriptionPlanEntity)
            .GetProperty(nameof(SubscriptionPlanEntity.SubscriptionPlanID))!
            .SetValue(entity, "sub-plan-unit-write-test-001");
        typeof(SubscriptionPlanEntity)
            .GetProperty(nameof(SubscriptionPlanEntity.Name))!
            .SetValue(entity, "Test Pro Plan Write Adapter");
        typeof(SubscriptionPlanEntity)
            .GetProperty(nameof(SubscriptionPlanEntity.IncludesFullCatalog))!
            .SetValue(entity, true);
        typeof(SubscriptionPlanEntity)
            .GetProperty(nameof(SubscriptionPlanEntity.IsActive))!
            .SetValue(entity, true);
        return entity;
    }
}

