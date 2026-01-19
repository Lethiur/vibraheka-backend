namespace VibraHeka.Infrastructure.Persistence;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

/// <summary>
/// Provides a wrapper for the Amazon DynamoDB client and context, enabling interaction with the DynamoDB tables.
/// </summary>
public class ApplicationDynamoContext(IAmazonDynamoDB client)
{
    /// <summary>
    /// Gets the instance of <see cref="IDynamoDBContext"/> which is used to interact with the Amazon DynamoDB database.
    /// This property provides a high-level abstraction for accessing tables, executing queries, and performing database operations.
    /// </summary>
    public IDynamoDBContext Context { get; } = new DynamoDBContextBuilder().WithDynamoDBClient(() => client).Build();
}
