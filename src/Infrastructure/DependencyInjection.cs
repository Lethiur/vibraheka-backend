using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using Stripe;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Codes;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Persistence.S3;
using VibraHeka.Infrastructure.Services;
using VibraHeka.Infrastructure.Tracer;
using VibraHeka.Web.Logging;
using SubscriptionService = VibraHeka.Infrastructure.Services.SubscriptionService;


namespace VibraHeka.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration config, ConfigurationManager configurationManager )
    {
        builder.Services.AddOptions<AWSConfig>().Bind(builder.Configuration.GetSection("AWS"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services
            .AddOptions<StripeConfig>()
            .Bind(builder.Configuration.GetSection("Stripe"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
      
        builder.Services.AddDefaultAWSOptions(config.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonDynamoDB>();
        builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddAWSService<IAmazonCloudWatchLogs>();
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSConfig? awsConfig = builder.Configuration.GetSection("AWS").Get<AWSConfig>();

        CredentialProfileStoreChain amazonSimpleSystemsManagementConfig = new();
        amazonSimpleSystemsManagementConfig.TryGetAWSCredentials(awsConfig?.Profile, out AWSCredentials credentials);

        builder.Services.AddSingleton<ITracer, XRayTracer>();
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<
                IOptions<StripeConfig>>().Value);
        
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<
                IOptions<AWSConfig>>().Value);
        
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AWSConfig>>().Value);

        configurationManager.AddSystemsManager(options =>
        {
            options.Path = $"/{awsConfig?.SettingsNameSpace}/";
            options.ReloadAfter = TimeSpan.FromSeconds(2); 
            options.Optional = true;
        });
        
        builder.Services.Configure<AppSettingsEntity>(configurationManager);
        
        
        StripeConfig? stripeConfig = builder.Configuration
            .GetSection("Stripe")
            .Get<StripeConfig>();

        if (stripeConfig == null)
        {
            throw new Exception("Stripe configuration not found.");
        }
        
        StripeConfiguration.ApiKey = stripeConfig.SecretKey;

        builder.Services.AddSingleton<SubscriptionEntityMapper>();
        builder.Services.AddSingleton<VerificationCodeEntityMapper>();
        builder.Services.AddSingleton<UsersCodeMapper>();
        
        builder.Services.AddScoped<ICodeRepository, VerificationCodesRepository>();
        builder.Services.AddScoped<IUserCodeRepository, UserCodeRepository>();
        builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
        builder.Services.AddScoped<ApplicationDynamoContext>();

        builder.Services.AddScoped<IActionLogRepository, ActionLogRepository>();
        
        // Settings
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
        
        // Payments
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IPaymentRepository, PaymentsRepository>();
        
        // Subscription
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        
        // Email Templates
        builder.Services.AddScoped<IEmailTemplatesRepository, EmailTemplateRepository>();
        builder.Services.AddScoped<IEmailTemplatesService, EmailTemplateService>();
        
        // Email template storage
        builder.Services.AddScoped<IEmailTemplateStorageService, EmailTemplateStorageService>();
        builder.Services.AddScoped<IEmailTemplateStorageRepository, EmailTemplateStorageRepository>();
        
        // Privileges
        builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Users
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserCodeService, UserCodeService>();
        builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        
        builder.Services.AddSingleton(TimeProvider.System);
        
     
    }
}
