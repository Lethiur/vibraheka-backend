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
    protected AWSConfig ConfigMock;
    protected SubscriptionRepository Repository;

    [SetUp]
    public void SetUp()
    {
        ContextMock = new Mock<IDynamoDBContext>();
        ConfigMock = new AWSConfig
        {
            SubscriptionTable = "SubscriptionsTable",
            SubscriptionUserIdIndex = "User-Index"
        };

        Repository = new SubscriptionRepository(
            ConfigMock,
            ContextMock.Object,
            new SubscriptionEntityMapper(),
            new Mock<ILogger<SubscriptionRepository>>().Object);
    }
}
