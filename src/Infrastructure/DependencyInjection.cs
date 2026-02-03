using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Codes;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Common.Interfaces.Settings;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Persistence.S3;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration config, ConfigurationManager configurationManager )
    {
        builder.Services.Configure<AWSConfig>(builder.Configuration.GetSection("AWS"));
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AWSConfig>>().Value);

        configurationManager.AddSystemsManager(options =>
        {
            options.Path = "/VibraHeka/";
            options.ReloadAfter = TimeSpan.FromSeconds(2); 
            options.Optional = true;
        });
        
        builder.Services.Configure<AppSettingsEntity>(configurationManager);
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettingsEntity>>().Value);
        builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());
        
        
        builder.Services.AddScoped<ICodeRepository, VerificationCodesRepository>();
        builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
        builder.Services.AddScoped<ApplicationDynamoContext>();

        builder.Services.AddScoped<IActionLogRepository, ActionLogRepository>();
        
        // Settings
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
        
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
        
        builder.Services.AddSingleton(TimeProvider.System);
        
        builder.Services.AddDefaultAWSOptions(config.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonDynamoDB>();
        builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
        builder.Services.AddAWSService<IAmazonS3>();
    }
}
