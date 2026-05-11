using System.Net;
using Infrastructure.Rest.Client.IntegrationTests.Helpers;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Adapters;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.AdaptersTest.MeetingAdapterTest;

/// <summary>
/// Base class for MeetingAdapter integration tests using deterministic HTTP stubs.
/// Each public method test pre-enqueues an auth-token response followed by the operation response.
/// </summary>
public abstract class GenericMeetingAdapterTest : TestBase
{
    protected MeetingAdapter Adapter = default!;
    protected ZoomApiClient ApiClient = default!;
    protected ZoomAuthService AuthService = default!;

    [SetUp]
    public virtual void SetUp()
    {
        ApiClient = new ZoomApiClient(CreateTestLogger<ZoomApiClient>(), new HttpClient());
        AuthService = new ZoomAuthService(ApiClient, Options.Create(ZoomConfig));
        Adapter = new MeetingAdapter(
            AuthService,
            ApiClient,
            Options.Create(ZoomConfig),
            new ZoomMeetingMapper(),
            CreateTestLogger<MeetingAdapter>());
    }
}
