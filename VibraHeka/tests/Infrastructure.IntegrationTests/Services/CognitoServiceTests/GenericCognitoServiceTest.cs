using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.CognitoServiceTests;

[TestFixture]
public abstract class GenericCognitoServiceTest : TestBase
{
    protected IUserService _userService;
    private ILogger<UserService> _logger;
    private VerificationCodesRepository _verificationCodeRepository;

    [OneTimeSetUp]
    public void OneTimeSetUpChild()
    {
        base.OneTimeSetUp();
        _logger = NullLogger<UserService>.Instance;
        _userService = new UserService(_configuration, _logger);
        _faker = new Faker();
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile("Twingers") })).Build();
        _verificationCodeRepository =
            new VerificationCodesRepository(dynamoDbContext, _configuration);
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

        Result<string> registerResult = await _userService.RegisterUserAsync(email, password, fullName);
        Assert.That(registerResult.IsSuccess, Is.True);
        return registerResult.Value;
    }
}
