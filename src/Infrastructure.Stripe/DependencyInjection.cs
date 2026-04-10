using Infrastructure.Stripe.Subscriptions.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.Stripe;

public static class DependencyInjection
{
    public static void AddPaymentServices(this IHostApplicationBuilder builder, IConfiguration config,
        ConfigurationManager configurationManager)

    {
        builder.Services.AddOptions<StripeConfig>().Bind(config.GetSection("Stripe"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        StripeConfig? stripeConfig = config
            .GetSection("Stripe")
            .Get<StripeConfig>();

        if (stripeConfig == null)
        {
            throw new Exception("Stripe configuration not found.");
        }
        
        StripeConfiguration.ApiKey = stripeConfig.SecretKey;

        builder.Services.AddScoped<PaymentsPort, PaymentsAdapter>();

    }
}
