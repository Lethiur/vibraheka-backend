using Infrastructure.Persistence.Commerce.Adapters;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Repositories;
using Infrastructure.Persistence.Events.Adapters;
using Infrastructure.Persistence.Products.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Commerce.Ports.Out;
using VibraHeka.Domain.Events.Ports.Out;
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
        services.AddSingleton<OrderMapper>();
        services.AddSingleton<OrderLineMapper>();
    }
    
    private static void AttachDynamoServices(this IServiceCollection services)
    {
        services.AddScoped<IEventRepositoryPort, EventAdapter>();
        services.AddScoped<IOrderPort, OrderAdapter>();
        services.AddScoped<IOrderLinePort, OrderAdapter>();
        services.AddScoped<IProductPort, ProductAdapter>();
    }

    private static void AttachRepositories(this IServiceCollection services)
    {
        services.AddSingleton<OrderLineRepository>();
        services.AddSingleton<OrderRepository>();
    }
}
