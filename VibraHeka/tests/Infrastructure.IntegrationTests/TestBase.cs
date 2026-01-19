using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;
using VibraHeka.Domain.Entities;

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
        
        string templatesTable = Environment.GetEnvironmentVariable("TEST_EMAIL_TEMPLATE_TABLE")
                                ?? throw new InvalidOperationException("TEST_EMAIL_TEMPLATE_TABLE environment variable is required");

        IConfigurationBuilder configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dynamo:CodesTable"] = verificationCodesTable,
                ["Cognito:UserPoolId"] = userPoolId,
                ["Cognito:ClientId"] = clientId,
                ["Dynamo:UsersTable"] = usersTable,
                ["Dynamo:EmailTemplatesTable"] = templatesTable,
                ["AppSettingsEntity:VerificationEmailTemplate"] = "",
                ["AppSettingsEntity:EmailForResetPassword"] = "",
                ["AWS:Region"] = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1",
                ["AWS:Profile"] = "Twingers"
            })
            .AddSystemsManager(options =>
            {
                options.Path = "/VibraHeka/";
                options.Optional = false;
            });
        _configuration = configBuilder.Build();
        return _configuration;
    }
    
    protected IDynamoDBContext CreateDynamoDBContext()
    {
    
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile("Twingers") })).Build();
        
        return dynamoDbContext;
    }
    
    protected AppSettingsEntity CreateAppSettings()
    {
        var settings = new AppSettingsEntity();
        _configuration.Bind(settings); // Ahora Bind encontrará las claves del diccionario o de SSM
        return settings;
    }
    
    protected void RefreshAppSettings(AppSettingsEntity settings)
    {
        if (_configuration is IConfigurationRoot root)
        {
            root.Reload();
        }
        _configuration.Bind(settings);
    }
    
    
    

}
