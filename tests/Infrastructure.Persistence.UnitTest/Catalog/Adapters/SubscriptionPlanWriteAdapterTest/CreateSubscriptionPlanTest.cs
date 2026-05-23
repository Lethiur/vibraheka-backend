using System.ComponentModel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Catalog.Models;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Catalog.Adapters.SubscriptionPlanWriteAdapterTest;

[TestFixture]
[NUnit.Framework.Category("Unit")]
public sealed class CreateSubscriptionPlanTest : GenericSubscriptionPlanWriteAdapterTest
{
    [Test]
    [DisplayName("Should create transactional write with correct table, add the mapped model once, and return a non-null operation wrapper")]
    public void ShouldCreateTransactionalWriteOperationWithCorrectTableAndMappedModel()
    {
        // Given: a valid SubscriptionPlanEntity with known Name and IncludesFullCatalog values
        SubscriptionPlanEntity subscriptionPlan = BuildDefaultSubscriptionPlanEntity();
        
        // And: Some mocking
        ContextMock.Setup(c => c.CreateTransactWrite<SubscriptionPlanDBModel>())
            .Returns(TransactWriteMock.Object);

        // When: CreateSubscriptionPlan is called on the adapter
        ITransactionalWriteOperation result = Adapter.CreateSubscriptionPlan(subscriptionPlan);
        
        // Then: AddSaveItem is called once with the model produced by the mapper (key fields match the domain entity)
        TransactWriteMock.Verify(
            x => x.AddSaveItem(
                It.Is<SubscriptionPlanDBModel>(m =>
                    m.Name == subscriptionPlan.Name &&
                    m.IncludesFullCatalog == subscriptionPlan.IncludesFullCatalog &&
                    m.IsActive == subscriptionPlan.IsActive)),
            Times.Once,
            $"Expected AddSaveItem called once with SubscriptionPlanDBModel where Name='{subscriptionPlan.Name}' and IncludesFullCatalog='{subscriptionPlan.IncludesFullCatalog}'");

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

