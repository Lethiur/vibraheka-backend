using Microsoft.AspNetCore.Mvc;
using VibraHeka.Web.Mappers;

namespace VibraHeka.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddSingleton<SubscriptionMapper>();
        
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "VibraHeka API";
        });
    }
}
