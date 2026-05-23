using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.ActionLogRepositoryTest;

public abstract class GenericActionLogRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected Mock<IAmazonDynamoDB> DynamoDbClientMock;
    protected ActionLogRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDbClientMock = new Mock<IAmazonDynamoDB>();
        Repository = new ActionLogRepository(ContextMock.Object, DynamoDbClientMock.Object, new Mock<ILogger<ActionLogRepository>>().Object);
    }
}
