using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Infrastructure.AWS.DynamoDB.Users.Adapters;
using Infrastructure.AWS.DynamoDB.Users.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Adapters.ActionLogAdapterTest;

public abstract class GenericActionLogRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected Mock<IAmazonDynamoDB> DynamoDBClientMock;
    protected Mock<IOptionsMonitor<AWSConfig>> ConfigMock;
    protected ActionLogAdapter Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        DynamoDBClientMock = new Mock<IAmazonDynamoDB>();
        ConfigMock = new Mock<IOptionsMonitor<AWSConfig>>();
        ConfigMock.Setup(c => c.CurrentValue).Returns(new AWSConfig { ActionLogTable = "ActionLogsTable" });
        Repository = new ActionLogAdapter(ContextMock.Object, DynamoDBClientMock.Object, ConfigMock.Object, new Mock<ILogger<ActionLogAdapter>>().Object, new ActionLogMapper());
    }
}
