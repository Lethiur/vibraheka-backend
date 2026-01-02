using Bogus;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;

namespace VibraHeka.Infrastructure.IntegrationTests;

public abstract class TestBase
{
    protected IConfiguration _configuration;
    protected Faker _faker;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        new EnvLoader().Load();
        _configuration = CreateTestConfiguration();
        _faker = new Faker();
    }

    protected IConfiguration CreateTestConfiguration()
    {
        string userPoolId = Environment.GetEnvironmentVariable("TEST_COGNITO_USER_POOL_ID")
                            ?? throw new InvalidOperationException(
                                "COGNITO_USER_POOL_ID environment variable is required");

        string clientId = Environment.GetEnvironmentVariable("TEST_COGNITO_CLIENT_ID")
                          ?? throw new InvalidOperationException("COGNITO_CLIENT_ID environment variable is required");

        string verificationCodesTable = Environment.GetEnvironmentVariable("TEST_DYNAMO_CODES_TABLE")
                                        ?? throw new InvalidOperationException(
                                            "TEST_DYNAMO_CODES_TABLE environment variable is required");

        string usersTable = Environment.GetEnvironmentVariable("TEST_DYNAMO_USERS_TABLE") ??
                            throw new InvalidOperationException(
                                "TEST_DYNAMO_USERS_TABLE environment variable is required");

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dynamo:CodesTable"] = verificationCodesTable,
                ["Cognito:UserPoolId"] = userPoolId,
                ["Cognito:ClientId"] = clientId,
                ["Dynamo:UsersTable"] = usersTable,
                ["AWS:Region"] = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1",
                ["AWS:Profile"] = "Twingers"
            });
        _configuration = configBuilder.Build();
        return _configuration;
    }
}
