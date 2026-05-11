using Infrastructure.Rest.Client.Stripe.Adapter;
using Infrastructure.Rest.Client.Stripe.Client;
using Infrastructure.Rest.Client.Stripe.Mappers;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Adapters;
using Infrastructure.Rest.Client.Zoom.Config;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Orders.Ports.Out;

namespace Infrastructure.Rest.Client;

public static class DependencyInjection
{
    private const string ZoomConfigKey = "Zoom";

    public static void AddRestClientServices(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AttachConfiguration(config);
        builder.Services.AttachMappers();
        builder.Services.AttachZoomServices();
        builder.Services.AttachStripeServices();
    }


    private static void AttachConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ZoomConfig>().Bind(configuration.GetSection(ZoomConfigKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
    }

    private static void AttachMappers(this IServiceCollection services)
    {
        services.AddScoped<ZoomMeetingMapper>();
        services.AddScoped<StripeMapper>();
    }

    private static void AttachZoomServices(this IServiceCollection services)
    {
        services.AddHttpClient<ZoomApiClient>();
        services.AddScoped<StripeAPIClient>();
        services.AddScoped<ZoomAuthService>();
        services.AddScoped<IEventMeetingPort, MeetingAdapter>();
    }
    
    private static void AttachStripeServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentsPort, PaymentsAdapter>();
    }
}
