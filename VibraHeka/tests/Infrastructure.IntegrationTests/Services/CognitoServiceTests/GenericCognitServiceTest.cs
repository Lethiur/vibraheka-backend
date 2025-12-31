using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using CSharpFunctionalExtensions;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.CognitoServiceTests;

[TestFixture]
public abstract class GenericCognitServiceTest
{
    protected ICognitoService _cognitoService;
    private IConfiguration _configuration;
    private ILogger<CognitoService> _logger;
    protected Faker _faker;
    private VerificationCodesRepository _verificationCodeRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        new EnvLoader().Load();
        _configuration = CreateTestConfiguration();
        _logger = NullLogger<CognitoService>.Instance;
        _cognitoService = new CognitoService(_configuration, _logger);
        _faker = new Faker();
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile("Twingers") })).Build();
        _verificationCodeRepository =
            new VerificationCodesRepository(dynamoDbContext, _configuration);
    }

    private static IConfiguration CreateTestConfiguration()
    {
        string userPoolId = Environment.GetEnvironmentVariable("TEST_COGNITO_USER_POOL_ID")
                            ?? throw new InvalidOperationException(
                                "COGNITO_USER_POOL_ID environment variable is required");

        string clientId = Environment.GetEnvironmentVariable("TEST_COGNITO_CLIENT_ID")
                          ?? throw new InvalidOperationException("COGNITO_CLIENT_ID environment variable is required");

        string verificationCodesTable = Environment.GetEnvironmentVariable("TEST_DYNAMO_CODES_TABLE")
                                        ?? throw new InvalidOperationException(
                                            "TEST_DYNAMO_CODES_TABLE environment variable is required");

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dynamo:CodesTable"] = verificationCodesTable,
                ["Cognito:UserPoolId"] = userPoolId,
                ["Cognito:ClientId"] = clientId,
                ["AWS:Region"] = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1",
                ["AWS:Profile"] = "Twingers"
            });

        return configBuilder.Build();
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

        Result<string> registerResult = await _cognitoService.RegisterUserAsync(email, password, fullName);
        Assert.That(registerResult.IsSuccess, Is.True);
        return registerResult.Value;
    }
}
