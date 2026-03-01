using Amazon.DynamoDBv2.DataModel;
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
        _logger = NullLogger<IPrivilegeService>.Instance;
        IDynamoDBContext dynamoDbContext = CreateDynamoDBContext();
        _userRepository = new UserRepository(dynamoDbContext, _configuration);
        _actionLogRepository = new ActionLogRepository(dynamoDbContext, _configuration, CreateTestLogger<ActionLogRepository>());
        PrivilegeService = new Infrastructure.Services.PrivilegeService(_userRepository, _actionLogRepository, _logger);
    }
}

