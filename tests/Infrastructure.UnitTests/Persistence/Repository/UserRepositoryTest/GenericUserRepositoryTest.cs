using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

public abstract class GenericUserRepositoryTest
{

    protected Mock<IDynamoDBContext> ContextMock;
    protected Mock<IAmazonDynamoDB> ClientMock;
    protected Mock<ILogger<UserRepository>> LoggerMock;
    protected AWSConfig ConfigMock;
    protected UserRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ClientMock = new Mock<IAmazonDynamoDB>();
        LoggerMock = new Mock<ILogger<UserRepository>>();
        ConfigMock = new AWSConfig()
        {
            UsersTable = "TestUsersTable"
        };

        Repository = new UserRepository(ContextMock.Object, ClientMock.Object, ConfigMock, LoggerMock.Object);
    }
}
