using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.UserRepositoryTest;

public abstract class GenericUserRepositoryTest
{

    protected Mock<IDynamoDBContext> ContextMock;
    protected Mock<IAmazonDynamoDB> ClientMock;
    protected Mock<ILogger<UserRepository>> LoggerMock;
    protected UserRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ClientMock = new Mock<IAmazonDynamoDB>();
        LoggerMock = new Mock<ILogger<UserRepository>>();

        Repository = new UserRepository(ContextMock.Object, ClientMock.Object, LoggerMock.Object);
    }
}
