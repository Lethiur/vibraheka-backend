using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace VibraHeka.Infrastructure.IntegrationTests.Persistence.Repository.UserRepositoryTest;

public abstract class GenericUserRepositoryTest
{
    protected IUserRepository _userRepository;
    protected IDynamoDBContext _dynamoContext;
    protected IConfiguration _configuration;
    protected Faker _faker;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        new EnvLoader().Load();
        _configuration = CreateTestConfiguration();
        _dynamoContext = CreateDynamoDBContext();
        _userRepository = new UserRepository(_dynamoContext, _configuration);
        _faker = new Faker();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dynamoContext.Dispose();
    }


    private IDynamoDBContext CreateDynamoDBContext()
    {
    
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile("Twingers") })).Build();
        
        return dynamoDbContext;
    }

    private static IConfiguration CreateTestConfiguration()
    {
        string usersTable = Environment.GetEnvironmentVariable("TEST_DYNAMO_USERS_TABLE")
                            ?? throw new InvalidOperationException("TEST_DYNAMO_USERS_TABLE environment variable is required");

        string region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1";

        Console.WriteLine($"Using DynamoDB configuration:");
        Console.WriteLine($"  UsersTable: {usersTable}");
        Console.WriteLine($"  Region: {region}");

        IConfigurationBuilder configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dynamo:UsersTable"] = usersTable,
                ["AWS:Region"] = region
            });

        return configBuilder.Build();
    }
    
    protected User CreateValidUser()
    {
        return new User(
            Guid.NewGuid().ToString(),
            _faker.Internet.Email(),
            _faker.Person.FullName
        )
        {
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
    }

    protected async Task CleanupUser(string userId)
    {
        try
        {
            await _dynamoContext.DeleteAsync<User>(userId);
            Console.WriteLine($"Cleanup: Deleted user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup user {userId}: {ex.Message}");
        }
    }


}
