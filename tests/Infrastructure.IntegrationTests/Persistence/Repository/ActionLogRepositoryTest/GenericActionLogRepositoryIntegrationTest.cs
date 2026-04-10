using Amazon.DynamoDBv2.DataModel;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.ActionLogRepositoryTest;

public abstract class GenericActionLogRepositoryIntegrationTest : TestBase
{
    protected IDynamoDBContext _dynamoDbContext;
    protected IActionLogRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbContext = CreateDynamoDBContext();
        _repository = new ActionLogRepository(
            _dynamoDbContext,
            _configuration,
            CreateTestLogger<ActionLogRepository>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }
}

