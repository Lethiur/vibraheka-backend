using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.AdaptersTest.MeetingAdapterTest;

[TestFixture]
public sealed class ScheduleMeetingAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success with join URL and meeting ID when auth and Zoom create-meeting both succeed")]
    public async Task ShouldReturnSuccessWhenAuthAndMeetingCreationSucceed()
    {
        // When: ScheduleMeetingAsync is called
        Result<CreateEventResult> result = await Adapter.ScheduleMeetingAsync(new CreateEventModel
        {
            Duration = 60,
            EventTimezone = "Europe/London",
            StartDate = DateTime.UtcNow.AddHours(1),
            Name = "Test Meeting for DeleteMetingAsyncTest",
            EventPassword = "Test1234"
        }, CancellationToken.None);

        // Then: result is success with valid meeting data mapped from the stub response
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.JoinURL, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty join URL from the stub response");

        // And: Delete meeting  
        await Adapter.DeleteMetingAsync(result.Value.EventID, CancellationToken.None);
    }
}
