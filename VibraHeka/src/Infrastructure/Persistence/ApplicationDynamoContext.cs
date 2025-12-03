namespace VibraHeka.Infrastructure.Persistence;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

public class ApplicationDynamoContext(IAmazonDynamoDB client)
{
    public IDynamoDBContext Context { get; } = new DynamoDBContextBuilder().WithDynamoDBClient(() => client).Build();
}
