using Microsoft.AspNetCore.Mvc;
using VibraHeka.Web.Controllers.Auth;
using VibraHeka.Web.Controllers.Users;
using VibraHeka.Web.Mappers;
using SubscriptionMapper = VibraHeka.Web.Controllers.Subscriptions.SubscriptionMapper;

namespace VibraHeka.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<SubscriptionMapper>();
        builder.Services.AddSingleton<AuthMapper>();
        builder.Services.AddSingleton<UserMapper>();
        builder.Services.AddSingleton<OrderRequestMapper>();
        builder.Services.AddSingleton<CreateEventMapper>();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "VibraHeka API";
        });
    }
}
