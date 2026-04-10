using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Subscriptions.Services;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Domain;

public static class DependencyInjection
{
    public static void AddDomainServices(this IHostApplicationBuilder builder, IConfiguration config,
        ConfigurationManager configurationManager)

    {
        builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        builder.Services.AddScoped<SubscriptionService>();
    }
}
