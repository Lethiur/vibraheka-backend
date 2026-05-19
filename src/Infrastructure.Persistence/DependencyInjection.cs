using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Repositories;
using Infrastructure.Persistence.Commerce.Adapters;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Repositories;
using Infrastructure.Persistence.Events.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Commerce.Ports.Out;
using VibraHeka.Domain.Events.Ports.Out;

namespace Infrastructure.Persistence;

public static class DependencyInjection
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMappers();
        builder.Services.AttachDynamoServices();
        builder.Services.AttachRepositories();
    }

    private static void AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<OrderMapper>();
        services.AddSingleton<OrderLineMapper>();
        services.AddSingleton<ProductEntityMapper>();
        services.AddSingleton<SellableItemEntityMapper>();
        services.AddSingleton<SellableItemPriceEntityMapper>();
        services.AddSingleton<SubscriptionPlanEntityMapper>();
    }

    private static void AttachDynamoServices(this IServiceCollection services)
    {
        services.AddSingleton<IAtomicWriteStore, DynamoAtomicWriteStore>();
        services.AddSingleton<IEventRepositoryPort, EventAdapter>();
        services.AddSingleton<IOrderPort, OrderAdapter>();
        services.AddSingleton<IOrderLinePort, OrderAdapter>();
        services.AddSingleton<IProductWritePort, ProductWriteAdapter>();
        services.AddSingleton<ISellableItemWritePort, SellableItemWriteAdapter>();
        services.AddSingleton<ISellableItemPriceWritePort, SellableItemPriceWriteAdapter>();
        services.AddSingleton<ISubscriptionPlanWritePort, SubscriptionPlanWriteAdapter>();
        services.AddSingleton<ISellableItemPort, SellableItemAdapter>();
        services.AddSingleton<ISellableItemPricePort, SellableItemPriceAdapter>();
    }

    private static void AttachRepositories(this IServiceCollection services)
    {
        services.AddSingleton<OrderLineRepository>();
        services.AddSingleton<OrderRepository>();
        services.AddSingleton<SellableItemRepository>();
        services.AddSingleton<SellableItemPriceRepository>();
    }
}
