using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SimpleEmailV2;
using Amazon.SimpleSystemsManagement;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stripe;
using VibraHeka.Domain.Common.Interfaces;
#if DEBUG
using VibraHeka.Domain.Common.Interfaces.Codes;
#endif
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Emails;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Persistence.S3;
using VibraHeka.Infrastructure.Services;
using SubscriptionService = VibraHeka.Infrastructure.Services.SubscriptionService;


namespace VibraHeka.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration config, ConfigurationManager configurationManager)
    {
        configurationManager.AddInfrastructureConfiguration(config);
        builder.Services.AddInfrastructureServices(config);
    }

    private static void AddInfrastructureConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IConfiguration configuration)
    {
        AWSConfig? awsConfig = configuration.GetSection("AWS").Get<AWSConfig>();

        if (!string.IsNullOrWhiteSpace(awsConfig?.SettingsNameSpace))
        {
            configurationBuilder.AddSystemsManager(options =>
            {
                options.Path = $"/{awsConfig.SettingsNameSpace}/";
                options.ReloadAfter = TimeSpan.FromSeconds(2);
                options.Optional = true;
            });
        }
    }

    private static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AWSConfig>().Bind(configuration.GetSection("AWS"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AWSLoggingConfig>().Bind(configuration.GetSection("AWSLogging"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<StripeConfig>()
            .Bind(configuration.GetSection("Stripe"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddAWSService<IAmazonSimpleEmailServiceV2>();
        services.AddAWSService<IAmazonSimpleSystemsManagement>();
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonCloudWatchLogs>();
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSConfig? awsConfig = configuration.GetSection("AWS").Get<AWSConfig>();

        CredentialProfileStoreChain amazonSimpleSystemsManagementConfig = new();
        amazonSimpleSystemsManagementConfig.TryGetAWSCredentials(awsConfig?.Profile, out AWSCredentials credentials);

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AWSLoggingConfig>>().Value);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AWSConfig>>().Value);
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<StripeConfig>>().Value);
        services.AddSingleton<EmailClient>();
        services.Configure<AppSettingsEntity>(configuration);
        services.Configure<AWSLoggingConfig>(configuration.GetSection("AWSLogging"));
        services.Configure<StripeConfig>(configuration.GetSection("Stripe"));

        StripeConfig? stripeConfig = configuration
            .GetSection("Stripe")
            .Get<StripeConfig>();

        if (stripeConfig == null)
        {
            throw new Exception("Stripe configuration not found.");
        }

        StripeConfiguration.ApiKey = stripeConfig.SecretKey;

        services.AddSingleton<SubscriptionEntityMapper>();
        services.AddSingleton<RecordingEntityMapper>();
        services.AddSingleton<VerificationCodeEntityMapper>();
        services.AddSingleton<UsersCodeMapper>();
#if DEBUG
        services.AddSingleton<ICodeRepository, VerificationCodesRepository>();
#endif
        services.AddSingleton<IUserCodeRepository, UserCodeRepository>();
        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
        services.AddSingleton<ApplicationDynamoContext>();

        services.AddSingleton<IActionLogRepository, ActionLogRepository>();

        // Settings
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        // Payments
        services.AddSingleton<IPaymentService, PaymentService>();
        services.AddSingleton<IPaymentRepository, PaymentsRepository>();

        // Subscription
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
        services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();

        // Email Templates
        services.AddSingleton<IEmailTemplatesRepository, EmailTemplateRepository>();
        services.AddSingleton<IEmailTemplatesService, EmailTemplateService>();

        // Email template storage
        services.AddSingleton<IEmailTemplateStorageService, EmailTemplateStorageService>();
        services.AddSingleton<IEmailTemplateStorageRepository, EmailTemplateStorageRepository>();

        // Privileges
        services.AddSingleton<IPrivilegeService, PrivilegeService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        // Users
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IUserCodeService, UserCodeService>();
        services.AddSingleton<IPasswordResetTokenService, PasswordResetTokenService>();

        // Recordings
        services.AddSingleton<IRecordingRegistryPort, RecordingRepository>();
        services.AddSingleton<IRecordingStoragePort, RecordingStorageRepository>();

        services.AddSingleton(TimeProvider.System);


    }
}
