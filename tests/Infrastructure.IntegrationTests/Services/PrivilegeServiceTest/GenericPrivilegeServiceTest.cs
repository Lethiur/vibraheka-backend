using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using DotEnv.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PrivilegeServiceTest;

public abstract class GenericPrivilegeServiceTest : TestBase
{
    protected IPrivilegeService PrivilegeService;

    protected ILogger<IPrivilegeService> _logger;
    
    protected IUserRepository _userRepository;
    
    protected IActionLogRepository _actionLogRepository;
    
    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        new EnvLoader().Load();
        _configuration = CreateTestConfiguration();
        _logger = NullLogger<IPrivilegeService>.Instance;
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig { Profile = new Profile("Twingers") })).Build();
        _userRepository = new UserRepository(dynamoDbContext, _configuration);
        _actionLogRepository = new ActionLogRepository(dynamoDbContext, _configuration);
        PrivilegeService = new Infrastructure.Services.PrivilegeService(_userRepository, _actionLogRepository, _logger);
        _faker = new Faker();
        
    }
}
