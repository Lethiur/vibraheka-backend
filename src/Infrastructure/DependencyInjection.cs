using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.User.Services;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Services;


namespace VibraHeka.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration config, ConfigurationManager configurationManager )
    {
        builder.Services.AddInfrastructureServices(config);
    }
    
    private static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddOptions<AWSLoggingConfig>().Bind(configuration.GetSection("AWSLogging"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        AWSSDKHandler.RegisterXRayForAllServices();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AWSLoggingConfig>>().Value);
       
        services.Configure<AWSLoggingConfig>(configuration.GetSection("AWSLogging"));
        services.AddSingleton(TimeProvider.System);
        
     
    }
}
