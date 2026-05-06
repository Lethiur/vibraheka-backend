using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom;
using Infrastructure.Rest.Client.Zoom.Adapters;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Services;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.AdaptersTest.MeetingAdapterTest;

[TestFixture]
public class ScheduleMeetingAsyncTest : TestBase
{
    [Test]
    public async Task ShouldCreateMeeting()
    {
        // Given: A meeting adapter
        ZoomApiClient client = new(CreateTestLogger<ZoomApiClient>(), new HttpClient());
        MeetingAdapter adapter = new MeetingAdapter(new ZoomAuthService(client, Options.Create(ZoomConfig)),
            client,
            Options.Create(ZoomConfig),
            new ZoomMeetingMapper(),
            CreateTestLogger<MeetingAdapter>());
        
        // When: A meeting is scheduled
        CreateEventModel createEventModel = new()
        {
            Duration = 65,
            StartDate = DateTime.UtcNow.AddMinutes(10),
            Name = "Test Meeting",
            EventTimezone = "Europe/Madrid",
            EventPassword = "NoVeASlOKO"
        };
        Result<CreateEventResult> scheduleMeetingAsync = await adapter.ScheduleMeetingAsync(createEventModel, CancellationToken.None);
        
        // Then: There should be valid data
        Assert.That(scheduleMeetingAsync.IsSuccess, Is.True);
        CreateEventResult result = scheduleMeetingAsync.Value;
        Assert.That(result.JoinURL, Is.Not.Null);
    }
}
