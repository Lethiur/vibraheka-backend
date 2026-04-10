using Amazon.CloudWatchLogs;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Infrastructure.AWS.Cognito.User.Adapters;
using Infrastructure.AWS.DynamoDB.EmailTemplates.Adapters;
using Infrastructure.AWS.DynamoDB.EmailTemplates.Mappers;
using Infrastructure.AWS.DynamoDB.PasswordResetToken.Adapters;
using Infrastructure.AWS.DynamoDB.PasswordResetToken.Mappers;
using Infrastructure.AWS.DynamoDB.Subscriptions.Adapters;
using Infrastructure.AWS.DynamoDB.Subscriptions.Mappers;
using Infrastructure.AWS.DynamoDB.Users.Adapters;
using Infrastructure.AWS.DynamoDB.Users.Mappers;
using Infrastructure.AWS.DynamoDB.VerificationCodes.Adapters;
using Infrastructure.AWS.DynamoDB.VerificationCodes.Mappers;
using Infrastructure.AWS.s3.EmailTemplates;
using Infrastructure.AWS.SSM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.EmailTemplates.Entities;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Domain.User.Ports.Out;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence;

namespace Infrastructure.AWS;

public static class DependencyInjection
{
    public static void AddCloudServices(this IHostApplicationBuilder builder, IConfiguration config,
        ConfigurationManager configurationManager)

    {
        builder.Services.AddOptions<AWSConfig>().Bind(config.GetSection("AWS"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
        builder.Services.AddScoped<ApplicationDynamoContext>();
        builder.Services.AddDefaultAWSOptions(config.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonDynamoDB>();
        builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddAWSService<IAmazonCloudWatchLogs>();

        builder.Services.AddScoped<UserPort, UserAdapter>();
        
        builder.Services.AddSingleton<UserProfileMapper>();
        builder.Services.AddScoped<UserProfilePort, UserProfileAdapter>();

        builder.Services.AddSingleton<EmailTemplateMapper>();
        builder.Services.AddScoped<EmailTemplatePort, EmailTemplateAdapter>();
        builder.Services.AddScoped<EmailTemplateContentPort, EmailTemplateContentAdapter>();
        builder.Services.AddScoped<EmailTemplateConfigPort, EmailTemplateConfigAdapter>();
        
        builder.Services.AddSingleton<UsersCodeMapper>();
        builder.Services.AddScoped<PasswordResetTokenPort, PasswordResetTokenAdapter>();

        builder.Services.AddSingleton<SubscriptionEntityMapper>();
        builder.Services.AddScoped<SubscriptionPort, SubscriptionAdapter>();

        builder.Services.AddSingleton<ActionLogMapper>();
        builder.Services.AddScoped<ActionLogPort, ActionLogAdapter>();
        
        
        builder.Services.AddScoped<UserPrivilegePort, PrivilegeAdapter>();
        
        #if DEBUG
        builder.Services.AddSingleton<VerificationCodeEntityMapper>();
        builder.Services.AddScoped<UserCodePort, UserCodeAdapter>();
        #endif
        
        configurationManager.AddSsmConfiguration(config);
        builder.Services.Configure<AppSettingsEntity>(config);
    }

    private static IConfigurationBuilder AddSsmConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IConfiguration configuration)
    {
        AWSConfig? awsConfig = configuration.GetSection("AWS").Get<AWSConfig>();

        if (string.IsNullOrWhiteSpace(awsConfig?.SettingsNameSpace))
        {
            return configurationBuilder;
        }

        configurationBuilder.AddSystemsManager(options =>
        {
            options.Path = $"/{awsConfig.SettingsNameSpace}/";
            options.ReloadAfter = TimeSpan.FromSeconds(2);
            options.Optional = true;
        });

        return configurationBuilder;
    }
}
