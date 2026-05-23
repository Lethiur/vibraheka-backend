using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using Moq;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.UnitTests.Persistence.Repository.SubscriptionRepositoryTest;

public abstract class GenericSubscriptionRepositoryTest
{
    protected Mock<IDynamoDBContext> ContextMock;
    protected Mock<IAmazonDynamoDB> ClientMock;
    protected AWSConfig ConfigMock;
    protected SubscriptionRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ClientMock = new Mock<IAmazonDynamoDB>();
        ConfigMock = new AWSConfig
        {
            SubscriptionUserIdIndex = "User-Index"
        };

        Repository = new SubscriptionRepository(
            ConfigMock,
            ClientMock.Object,
            ContextMock.Object,
            new SubscriptionEntityMapper(),
            new Mock<ILogger<SubscriptionRepository>>().Object);
    }
}
