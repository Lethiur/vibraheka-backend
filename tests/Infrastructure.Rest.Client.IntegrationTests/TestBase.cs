using System.ComponentModel.DataAnnotations;
using Infrastructure.Rest.Client.Zoom.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static System.ComponentModel.DataAnnotations.Validator;


namespace Infrastructure.Rest.Client.IntegrationTests;

public abstract class TestBase
{
    protected ZoomConfig ZoomConfig;
    private ILoggerFactory FactoryLogger;
    private IConfigurationRoot ConfigRoot;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {

        FactoryLogger = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        ConfigRoot = CreateTestConfiguration();
        ZoomConfig = CreateZoomConfig();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        FactoryLogger.Dispose();
    }

    protected ILogger<T> CreateTestLogger<T>()
    {
        if (FactoryLogger is null)
            throw new InvalidOperationException("LoggerFactory is not initialized.");

        return FactoryLogger.CreateLogger<T>();
    }

    private IConfigurationRoot CreateTestConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private ZoomConfig CreateZoomConfig()
    {
        ZoomConfig config = ConfigRoot.GetSection("Zoom").Get<ZoomConfig>() ?? throw new InvalidOperationException("Missing Zoom Config");
        ValidateObject(
            config, new ValidationContext(config), validateAllProperties: true);
        return config;
    }
}
