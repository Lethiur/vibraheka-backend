using Infrastructure.Persistence.Events.Adapters;
using Infrastructure.Persistence.Orders.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Orders.Ports.Out;

namespace Infrastructure.Persistence;

public static class DependencyInjection
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AttachDynamoServices();
    }
    
    public static void AttachDynamoServices(this IServiceCollection services)
    {
        services.AddScoped<IEventRepositoryPort, EventAdapter>();
        services.AddScoped<IOrderPort, OrderAdapter>();
    }
}
