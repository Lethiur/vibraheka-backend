using Infrastructure.Persistence.Events.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibraHeka.Domain.Events.Ports.Out;

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
    }
}
