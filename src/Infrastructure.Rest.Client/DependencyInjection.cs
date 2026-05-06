using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Config;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Rest.Client;

public static class DependencyInjection
{
    private const string ZoomConfigKey = "Zoom";
    
    public static void AddRestClientServices(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AttachConfiguration(config);
        builder.Services.AttachZoomServices();
    }


    private static void AttachConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ZoomConfig>().Bind(configuration.GetSection(ZoomConfigKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AttachZoomServices(this IServiceCollection services)
    {
        services.AddHttpClient<ZoomApiClient>();
        services.AddScoped<ZoomAuthService>();
        services.AddSingleton<ZoomMeetingMapper>();
    }
}
