using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Infrastructure.Persistence.Repository;

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
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ActionLogRepository>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }
}
