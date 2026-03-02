using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.ActionLogRepositoryTest;

public abstract class GenericActionLogRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected AWSConfig ConfigMock;
    protected ActionLogRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ConfigMock = new AWSConfig { ActionLogTable = "ActionLogsTable" };
        Repository = new ActionLogRepository(ContextMock.Object, ConfigMock, new Mock<ILogger<ActionLogRepository>>().Object);
    }
}
