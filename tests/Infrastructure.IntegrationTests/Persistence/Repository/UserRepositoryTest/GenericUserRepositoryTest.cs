using Amazon.DynamoDBv2.DataModel;
using Bogus;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

public abstract class GenericUserRepositoryTest : TestBase
{
    protected IUserRepository _userRepository;
    protected IDynamoDBContext _dynamoContext;
    
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        base.OneTimeSetUp();
        _dynamoContext = CreateDynamoDBContext();
        _userRepository = new UserRepository(_dynamoContext, _configuration);
        _faker = new Faker();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dynamoContext.Dispose();
    }

    protected async Task CleanupUser(string userID)
    {
        await base.CleanupUser(userID, _dynamoContext);
    }
}
