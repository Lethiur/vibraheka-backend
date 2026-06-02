using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Events.Models;

namespace VibraHeka.Infrastructure.Rest.Client.IntegrationTests.Zoom.AdaptersTest.MeetingAdapterTest;

[TestFixture]
public sealed class DeleteMetingAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success when auth and Zoom delete-meeting both succeed with 204")]
    public async Task ShouldReturnSuccessWhenAuthAndDeleteBothSucceed()
    {
        // Given: Meeting scheduled
        Result<CreateEventResult> scheduleMeetingAsync = await Adapter.ScheduleMeetingAsync(new CreateEventModel
        {
            Duration = 60,
            EventTimezone = "Europe/London",
            StartDate = DateTime.UtcNow.AddHours(1),
            Name = "Test Meeting for DeleteMetingAsyncTest",
            EventPassword = "Test1234"
        }, CancellationToken.None);

        // When: DeleteMetingAsync is called with a valid meeting ID
        Result<Unit> result = await Adapter.DeleteMetingAsync(scheduleMeetingAsync.Value.EventID, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
    }
}

