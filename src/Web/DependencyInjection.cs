using Microsoft.AspNetCore.Mvc;
using VibraHeka.Web.Controllers.Auth;
using VibraHeka.Web.Controllers.Catalog.Pricing;
using VibraHeka.Web.Controllers.Catalog.Recordings;
using VibraHeka.Web.Controllers.Catalog.SubscriptionPlans;
using VibraHeka.Web.Controllers.Commerce;
using VibraHeka.Web.Controllers.EmailTemplates;
using VibraHeka.Web.Controllers.Events;
using VibraHeka.Web.Controllers.Settings;
using VibraHeka.Web.Controllers.Users;

using SubscriptionMapper = VibraHeka.Web.Controllers.Subscriptions.SubscriptionMapper;

namespace VibraHeka.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<SubscriptionMapper>();
        builder.Services.AddSingleton<SubscriptionPlanMapper>();
        builder.Services.AddSingleton<AuthMapper>();
        builder.Services.AddSingleton<UserMapper>();
        builder.Services.AddSingleton<OrdersMapper>();
        builder.Services.AddSingleton<EventMapper>();
        builder.Services.AddSingleton<PricingMapper>();
        builder.Services.AddSingleton<RecordingMapper>();
        builder.Services.AddSingleton<EmailTemplateMapper>();
        builder.Services.AddSingleton<SettingsMapper>();
        
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "VibraHeka API";
        });
    }
}
