using System.ComponentModel.DataAnnotations;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using static System.ComponentModel.DataAnnotations.Validator;

namespace VibraHeka.Infrastructure.IntegrationTests;

public abstract class TestBase
{
    protected AWSConfig _configuration;
    protected Faker _faker;
    protected StripeConfig _stripeConfig;
    private IConfigurationRoot _config;
    private ILoggerFactory? _loggerFactory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _config = CreateTestConfiguration();
        _configuration = CreateAWSConfig();
        _stripeConfig = CreateStripeConfig();
        _faker = new Faker();
        Segment segment = new("VH-TEST");
        AWSXRayRecorder.Instance.TraceContext.SetEntity(segment);
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new NUnitLoggerProvider());
        });
    }

    [OneTimeTearDown]
    public void DisposeTestLoggerFactory()
    {
        _loggerFactory?.Dispose();
    }

    protected ILogger<T> CreateTestLogger<T>()
    {
        if (_loggerFactory is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        return _loggerFactory.CreateLogger<T>();
    }

    protected IConfigurationRoot CreateTestConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private AWSConfig CreateAWSConfig()
    {
        AWSConfig awsConfig = _config.GetSection("AWS").Get<AWSConfig>()
                              ?? throw new InvalidOperationException("Missing AWS config section.");
        ValidateObject(
            awsConfig, new ValidationContext(awsConfig), validateAllProperties: true);
        
        return awsConfig;
    }

    private StripeConfig CreateStripeConfig()
    {
        StripeConfig stripeConfig = _config.GetSection("Stripe").Get<StripeConfig>()
                                    ?? throw new InvalidOperationException("Missing Stripe config section.");

        ValidateObject(
            stripeConfig, new ValidationContext(stripeConfig), validateAllProperties: true);

        StripeConfiguration.ApiKey = stripeConfig.SecretKey;
        return stripeConfig;
    }

    protected IDynamoDBContext CreateDynamoDBContext()
    {
        DynamoDBContext dynamoDbContext = new DynamoDBContextBuilder().WithDynamoDBClient(() =>
                new AmazonDynamoDBClient(new AmazonDynamoDBConfig() { Profile = new Profile(_configuration.Profile) }))
            .Build();

        return dynamoDbContext;
    }

    protected AppSettingsEntity CreateAppSettings()
    {
        return new AppSettingsEntity();
    }
    
        
    protected UserEntity CreateValidUser()
    {
        return new UserEntity(
            Guid.NewGuid().ToString(),
            _faker.Internet.Email(),
            _faker.Person.FirstName
        )
        {
            LastName = _faker.Person.LastName,
            MiddleName = _faker.Person.UserName,
            PhoneNumber = "1234567890",
            Bio = _faker.Lorem.Paragraph(),
            TimezoneID = _faker.Address.Locale,
            Role = UserRole.User,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
    }
    
    protected async Task CleanupUser(string userId, IDynamoDBContext dynamoContext)
    {
        try
        {
            await dynamoContext.DeleteAsync<UserDBModel>(userId);
            Console.WriteLine($"Cleanup: Deleted user {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not cleanup user {userId}: {ex.Message}");
        }
    }
    
}
