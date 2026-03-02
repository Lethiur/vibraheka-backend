using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserCodeServiceTest;

public abstract class GenericUserCodeServiceIntegrationTest : TestBase
{
    protected IDynamoDBContext _dynamoDbContext;
    protected IUserCodeRepository _userCodeRepository;
    protected IUserCodeService _userCodeService;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _dynamoDbContext = CreateDynamoDBContext();
        _userCodeRepository = new UserCodeRepository(
            _configuration,
            _dynamoDbContext,
            new UsersCodeMapper(),
            CreateTestLogger<GenericDynamoRepository<UserCodeDBModel>>());
        _userCodeService = new UserCodeService(_userCodeRepository, CreateTestLogger<UserCodeService>());
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
