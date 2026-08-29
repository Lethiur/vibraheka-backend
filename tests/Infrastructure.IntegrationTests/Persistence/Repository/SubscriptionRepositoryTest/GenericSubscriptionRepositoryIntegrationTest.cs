using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Interfaces.Subscription;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.SubscriptionRepositoryTest;

public abstract class GenericSubscriptionRepositoryIntegrationTest : TestBase
{
    protected IDynamoDBContext _dynamoDbContext;
    protected ISubscriptionRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbContext = CreateDynamoDBContext();
        _repository = new SubscriptionRepository(
            _configuration,
            _client,
            _dynamoDbContext,
            new SubscriptionEntityMapper(),
            CreateTestLogger<SubscriptionRepository>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }
}

