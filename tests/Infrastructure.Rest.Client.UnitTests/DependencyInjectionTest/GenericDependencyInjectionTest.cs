using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Infrastructure.Rest.Client.UnitTests.DependencyInjectionTest;

public abstract class GenericDependencyInjectionTest
{
    protected ServiceCollection Services = default!;
    protected Mock<IHostApplicationBuilder> BuilderMock = default!;

    [SetUp]
    public virtual void SetUp()
    {
        Services = new ServiceCollection();
        BuilderMock = new Mock<IHostApplicationBuilder>();
        BuilderMock.SetupGet(b => b.Services).Returns(Services);
    }

    protected static IConfiguration BuildValidZoomConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Zoom:AccountID"] = "TestAccountID",
                ["Zoom:ClientID"] = "TestClientID",
                ["Zoom:ClientSecret"] = "TestClientSecret",
                ["Zoom:HostEmail"] = "host@example.com",
            })
            .Build();
    }

    protected static IConfiguration BuildEmptyConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
    }

    protected bool IsRegistered(Type serviceType)
    {
        return Services.Any(sd => sd.ServiceType == serviceType);
    }

    protected bool IsRegisteredWithLifetime(Type serviceType, ServiceLifetime lifetime)
    {
        return Services.Any(sd =>
            sd.ServiceType == serviceType &&
            sd.Lifetime == lifetime);
    }
}

