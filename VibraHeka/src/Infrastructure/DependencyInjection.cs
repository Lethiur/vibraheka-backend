using VibraHeka.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("VibraHekaDb");
        Guard.Against.Null(connectionString, message: "Connection string 'VibraHekaDb' not found.");

        
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
