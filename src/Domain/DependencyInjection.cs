using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Orders.Services;

namespace VibraHeka.Domain;

public static class DependencyInjection
{
    public static void AddDomainServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<CustomerService>();
    }
}
