using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Application.Abstractions.Transactions;

namespace Infrastructure.Persistence;

public class DynamoTransactionalWriteOperation(ITransactWrite item) : ITransactionalWriteOperation
{
    public readonly ITransactWrite Item = item;
}
