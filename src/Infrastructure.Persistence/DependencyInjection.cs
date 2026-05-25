using Infrastructure.Persistence.Catalog.Adapters;
using Infrastructure.Persistence.Catalog.Mappers;
using Infrastructure.Persistence.Catalog.Repositories;
using Infrastructure.Persistence.Commerce.Adapters;
using Infrastructure.Persistence.Commerce.Mappers;
using Infrastructure.Persistence.Commerce.Repositories;
using Infrastructure.Persistence.Events.Adapters;
using Infrastructure.Persistence.Events.Mappers;
using Infrastructure.Persistence.Events.Repositories;
using Infrastructure.Persistence.Payments.Adapters;
using Infrastructure.Persistence.Payments.Mappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Application.Catalog.Ports.Out;
using VibraHeka.Application.Commerce.Ports.Out;
using VibraHeka.Application.Payments.Ports.Out;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Domain.Commerce.Ports.Out;
using VibraHeka.Domain.Events.Ports.Out;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.Repository;

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
        services.AddSingleton<PaymentAttemptMapper>();
        services.AddSingleton<RecordingEntityMapper>();
        services.AddSingleton<EventEntityMapper>();
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
        services.AddSingleton<ISellableItemWritePort, SellableItemWriteAdapter>();
        services.AddSingleton<ISellableItemPriceWritePort, SellableItemPriceWriteAdapter>();
        services.AddSingleton<ISubscriptionPlanWritePort, SubscriptionPlanWriteAdapter>();
        services.AddSingleton<ISellableItemPort, SellableItemAdapter>();
        services.AddSingleton<ISellableItemPricePort, SellableItemPriceAdapter>();
        services.AddSingleton<IOrderWritePort, OrderWriteAdapter>();
        services.AddSingleton<IOrderLineWritePort, OrderLineWriteAdapter>();
        services.AddSingleton<IPaymentAttemptWritePort, PaymentAttemptWriteAdapter>();
        services.AddSingleton<IRecordingRegistryPort, RecordingsAdapter>();
        services.AddSingleton<IEventRepositoryPort, EventAdapter>();
    }

    private static void AttachRepositories(this IServiceCollection services)
    {
        services.AddSingleton<OrderLineRepository>();
        services.AddSingleton<OrderRepository>();
        services.AddSingleton<SellableItemRepository>();
        services.AddSingleton<SellableItemPriceRepository>();
        services.AddSingleton<RecordingRepository>();
        services.AddSingleton<EventRepository>();
    }
}

