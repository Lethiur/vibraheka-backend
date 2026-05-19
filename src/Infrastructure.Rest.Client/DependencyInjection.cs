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
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Payments.Ports.Out;

namespace Infrastructure.Rest.Client;

public static class DependencyInjection
{
    private const string ZoomConfigKey = "Zoom";

    public static void AddRestClientServices(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AttachConfiguration(config);
        builder.Services.AttachMappers();
        builder.Services.AttachAdapters();
        builder.Services.AttachZoomServices();
        builder.Services.AttachStripeServices();
    }


    private static void AttachConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ZoomConfig>().Bind(configuration.GetSection(ZoomConfigKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AttachAdapters(this IServiceCollection services)
    {
        services.AddSingleton<IProductCreationWritePort, CatalogAdapter>();
        services.AddSingleton<IEventMeetingPort, MeetingAdapter>();
        services.AddSingleton<IPaymentsPort, PaymentsAdapter>();
    }

    private static void AttachMappers(this IServiceCollection services)
    {
        services.AddSingleton<ZoomMeetingMapper>();
        services.AddSingleton<StripeMapper>();
    }

    private static void AttachZoomServices(this IServiceCollection services)
    {
        services.AddHttpClient<ZoomApiClient>();
        services.AddSingleton<ZoomAuthService>();
    }

    private static void AttachStripeServices(this IServiceCollection services)
    {
        services.AddSingleton<StripeAPIClient>();
    }
}
