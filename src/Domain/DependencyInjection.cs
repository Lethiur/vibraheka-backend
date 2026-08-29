using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Catalog.Services;
using VibraHeka.Domain.Payments.Services;

namespace VibraHeka.Domain;

public static class DependencyInjection
{
    public static void AddDomainServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CustomerService>();
        builder.Services.AddSingleton<SellableItemService>();
    }
}
