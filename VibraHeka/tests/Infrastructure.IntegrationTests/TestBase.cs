using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Bogus;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.IntegrationTests;

public abstract class TestBase
{
    protected AWSConfig _configuration;
    protected Faker _faker;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        new EnvLoader().Load();
        _configuration = CreateTestConfiguration();
        _faker = new Faker();
    }

    protected AWSConfig CreateTestConfiguration()
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

        string emailTemplatesBucketName = Environment.GetEnvironmentVariable("TEST_EMAIL_TEMPLATES_BUCKET_NAME") ?? throw new InvalidOperationException("TEST_EMAIL_TEMPLATES_BUCKET_NAME environment variable is required");
        
        _configuration = Options.Create(new AWSConfig()
        {
            CodesTable = verificationCodesTable,
            UserPoolId = userPoolId,
            ClientId = clientId,
            UsersTable = usersTable,
            EmailTemplatesTable = templatesTable,
            EmailTemplatesBucketName = emailTemplatesBucketName,
            Profile = Environment.GetEnvironmentVariable("AWS_PROFILE") ?? throw new InvalidOperationException("AWS_PROFILE environment variable is required")
        }).Value;
        return _configuration;
    }
    
    protected IDynamoDBContext CreateDynamoDBContext()
    {
    
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
            new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile(_configuration.Profile) })).Build();
        
        return dynamoDbContext;
    }
    
    protected AppSettingsEntity CreateAppSettings()
    {
        return new AppSettingsEntity();
    }
    
    
    
    

}
