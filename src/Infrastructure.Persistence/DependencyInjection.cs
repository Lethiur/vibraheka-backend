using Infrastructure.Persistence.Events.Adapters;
using Infrastructure.Persistence.Orders.Adapters;
using Infrastructure.Persistence.Orders.Mappers;
using Infrastructure.Persistence.Products.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Orders.Ports.Out;
using VibraHeka.Domain.Products.Ports.Out;

namespace Infrastructure.Persistence;

public static class DependencyInjection
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMappers();
        builder.Services.AttachDynamoServices();
    }

    private static void AddMappers(this IServiceCollection services)
    {
        services.AddScoped<OrderEntityMapper>();
    }
    
    public static void AttachDynamoServices(this IServiceCollection services)
    {
        services.AddScoped<IEventRepositoryPort, EventAdapter>();
        services.AddScoped<IOrderPort, OrderAdapter>();
        services.AddScoped<IProductPort, ProductAdapter>();
    }
}
