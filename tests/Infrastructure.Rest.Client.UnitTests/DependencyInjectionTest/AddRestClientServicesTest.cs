using System.ComponentModel;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Rest.Client.UnitTests.DependencyInjectionTest;

[TestFixture]
public sealed class AddRestClientServicesTest : GenericDependencyInjectionTest
{
    [Test]
    [DisplayName("Should register ZoomAuthService as scoped")]
    public void ShouldRegisterZoomAuthServiceAsScoped()
    {
        // Given: a valid configuration and a host application builder mock
        IConfiguration config = BuildValidZoomConfiguration();

        // When: AddRestClientServices is called
        DependencyInjection.AddRestClientServices(BuilderMock.Object, config);

        // Then: ZoomAuthService is registered as scoped
        bool isRegistered = IsRegisteredWithLifetime(typeof(ZoomAuthService), ServiceLifetime.Scoped);
        Assert.That(isRegistered, Is.True,
            "Expected ZoomAuthService to be registered as Scoped in the service collection");
        BuilderMock.VerifyGet(b => b.Services, Moq.Times.AtLeastOnce(),
            "Expected builder.Services to be accessed during registration");
        BuilderMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should register ZoomMeetingMapper as singleton")]
    public void ShouldRegisterZoomMeetingMapperAsSingleton()
    {
        // Given: a valid configuration and a host application builder mock
        IConfiguration config = BuildValidZoomConfiguration();

        // When: AddRestClientServices is called
        DependencyInjection.AddRestClientServices(BuilderMock.Object, config);

        // Then: ZoomMeetingMapper is registered as singleton
        bool isRegistered = IsRegisteredWithLifetime(typeof(ZoomMeetingMapper), ServiceLifetime.Singleton);
        Assert.That(isRegistered, Is.True,
            "Expected ZoomMeetingMapper to be registered as Singleton in the service collection");
        BuilderMock.VerifyGet(b => b.Services, Moq.Times.AtLeastOnce(),
            "Expected builder.Services to be accessed during registration");
        BuilderMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should register ZoomApiClient via HttpClient factory")]
    public void ShouldRegisterZoomApiClientViaHttpClientFactory()
    {
        // Given: a valid configuration and a host application builder mock
        IConfiguration config = BuildValidZoomConfiguration();

        // When: AddRestClientServices is called
        DependencyInjection.AddRestClientServices(BuilderMock.Object, config);

        // Then: ZoomApiClient is registered (via AddHttpClient typed client)
        bool isRegistered = IsRegistered(typeof(ZoomApiClient));
        Assert.That(isRegistered, Is.True,
            "Expected ZoomApiClient to be registered via AddHttpClient in the service collection");
        BuilderMock.VerifyGet(b => b.Services, Moq.Times.AtLeastOnce(),
            "Expected builder.Services to be accessed during registration");
        BuilderMock.VerifyNoOtherCalls();
    }

    [Test]
    [DisplayName("Should bind ZoomConfig from IConfiguration Zoom section")]
    public void ShouldBindZoomConfigFromConfiguration()
    {
        // Given: a configuration containing a Zoom section with valid values
        IConfiguration config = BuildValidZoomConfiguration();

        // When: AddRestClientServices is called
        DependencyInjection.AddRestClientServices(BuilderMock.Object, config);

        // Then: service provider can resolve IOptions<ZoomConfig> with bound values
        ServiceProvider provider = Services.BuildServiceProvider();
        Microsoft.Extensions.Options.IOptions<Infrastructure.Rest.Client.Zoom.Config.ZoomConfig> options =
            provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Infrastructure.Rest.Client.Zoom.Config.ZoomConfig>>();

        Assert.That(options.Value.AccountID, Is.EqualTo("TestAccountID"),
            $"Expected AccountID 'TestAccountID' but got '{options.Value.AccountID}'");
        Assert.That(options.Value.ClientID, Is.EqualTo("TestClientID"),
            $"Expected ClientID 'TestClientID' but got '{options.Value.ClientID}'");
        Assert.That(options.Value.ClientSecret, Is.EqualTo("TestClientSecret"),
            $"Expected ClientSecret 'TestClientSecret' but got '{options.Value.ClientSecret}'");
        Assert.That(options.Value.HostEmail, Is.EqualTo("host@example.com"),
            $"Expected HostEmail 'host@example.com' but got '{options.Value.HostEmail}'");
        BuilderMock.VerifyGet(b => b.Services, Moq.Times.AtLeastOnce(),
            "Expected builder.Services to be accessed during registration");
        BuilderMock.VerifyNoOtherCalls();
    }
}


