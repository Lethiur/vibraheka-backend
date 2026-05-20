using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Application.Abstractions.Transactions;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.DynamoAtomicWriteStoreTest;

public abstract class GenericDynamoAtomicWriteStoreTest
{
    protected Mock<IDynamoDBContext> ContextMock = default!;
    protected Mock<ILogger<DynamoAtomicWriteStore>> LoggerMock = default!;
    protected DynamoAtomicWriteStore Store = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        LoggerMock = new Mock<ILogger<DynamoAtomicWriteStore>>();
        Store = new DynamoAtomicWriteStore(ContextMock.Object, LoggerMock.Object);
    }

    protected static TransactionalWriteBatch BuildBatchWithOperations(int count, string idempotencyKey = "test-idem-key")
    {
        TransactionalWriteBatch batch = new(idempotencyKey);
        for (int i = 0; i < count; i++)
        {
            Mock<ITransactWrite> transactWriteMock = new();
            batch.Add(new DynamoTransactionalWriteOperation(transactWriteMock.Object));
        }

        return batch;
    }
}

