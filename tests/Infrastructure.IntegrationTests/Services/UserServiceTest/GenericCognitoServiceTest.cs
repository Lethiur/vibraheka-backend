using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public abstract class GenericCognitoServiceTest : TestBase
{
    protected IUserService UserService;
    private ILogger<UserService> Logger;
    protected IUserRepository UserRepository;
    
    private VerificationCodesRepository _verificationCodeRepository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UserService>();
        _faker = new Faker();
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile("Twingers") })).Build();
        _verificationCodeRepository =
            new VerificationCodesRepository(dynamoDbContext, _configuration, new VerificationCodeEntityMapper());
        UserRepository = new UserRepository(dynamoDbContext, _configuration);
        UserService = new UserService(_configuration, Logger, UserRepository);
    }



    protected string GenerateUniqueEmail(string prefix = "test-confirm@")
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string? randomSuffix = _faker.Random.AlphaNumeric(6);
        return $"{prefix.Replace("@", "")}-{timestamp}-{randomSuffix}@example.com";
    }

    protected async Task<VerificationCodeEntity> WaitForVerificationCode(string itemId, TimeSpan timeout)
    {
        DateTime startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            Result<VerificationCodeEntity> registerUserResponse = await _verificationCodeRepository.GetCodeFor(itemId);
            if (registerUserResponse.IsSuccess) return registerUserResponse.Value;

            await Task.Delay(500); // Wait before retrying
        }

        throw new TimeoutException("DynamoDB record was not available within the expected time.");
    }

    protected async Task<string> RegisterUser(string email)
    {
        // Given: A registered user
        const string password = "ValidPassword123!";
        const string fullName = "John Doe";

        Result<string> registerResult = await UserService.RegisterUserAsync(email, password, fullName);
        Assert.That(registerResult.IsSuccess, Is.True);
        return registerResult.Value;
    }
}
