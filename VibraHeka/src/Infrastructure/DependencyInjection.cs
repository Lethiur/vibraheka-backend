using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Infrastructure.Persistence;
using VibraHeka.Infrastructure.Persistence.Repository;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        builder.Services.AddScoped<ICodeRepository, VerificationCodesRepository>();
        builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
        builder.Services.AddScoped<ApplicationDynamoContext>();
        builder.Services.AddScoped<ICognitoService, CognitoService>();
        builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDefaultAWSOptions(config.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonDynamoDB>();
    }
}
