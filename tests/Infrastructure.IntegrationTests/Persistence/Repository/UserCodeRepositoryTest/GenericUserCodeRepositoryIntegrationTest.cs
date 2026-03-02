using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserCodeRepositoryTest;

public abstract class GenericUserCodeRepositoryIntegrationTest : TestBase
{
    protected IDynamoDBContext _dynamoDbContext;
    protected IUserCodeRepository _repository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbContext = CreateDynamoDBContext();
        _repository = new UserCodeRepository(
            _configuration,
            _dynamoDbContext,
            new UsersCodeMapper(),
            CreateTestLogger<GenericDynamoRepository<UserCodeDBModel>>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDownChild()
    {
        _dynamoDbContext.Dispose();
    }

    protected async Task CleanupTokenAsync(string tokenId)
    {
        try
        {
            await _dynamoDbContext.DeleteAsync<UserCodeDBModel>(
                tokenId,
                new DeleteConfig
                {
                    OverrideTableName = _configuration.UserCodesTable
                },
                CancellationToken.None);
        }
        catch
        {
            // Best effort cleanup for test records.
        }
    }
}
