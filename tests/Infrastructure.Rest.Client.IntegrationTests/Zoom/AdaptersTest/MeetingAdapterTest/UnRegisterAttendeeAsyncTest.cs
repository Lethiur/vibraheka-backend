using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.AdaptersTest.MeetingAdapterTest;

[TestFixture]
public sealed class UnRegisterAttendeeAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success when auth and Zoom unregister both succeed with 204")]
    public async Task ShouldReturnSuccessWhenAuthAndUnregisterSucceed()
    {
        // Given: auth returns a valid token and Zoom register returns 200 OK with registrant body
        Result<CreateEventResult> scheduleMeetingAsync = await Adapter.ScheduleMeetingAsync(new CreateEventModel
        {
            Duration = 60,
            EventTimezone = "Europe/London",
            StartDate = DateTime.UtcNow.AddHours(1),
            Name = "Test Meeting for DeleteMetingAsyncTest",
            EventPassword = "Test1234"
        }, CancellationToken.None);

        // And: Some model
        RegisterAttendeeModel model = new()
        {
            EventID = scheduleMeetingAsync.Value.EventID,
            RegistrantEmail = "mtesqtsdlc2@gmail.com",
            RegistrantLastName = "John",
            RegistrantName = "Doe"
        };

        // And: RegisterAttendeeAsync is called
        Result<RegisterAttendeeResult> registerResult = await Adapter.RegisterAttendeeAsync(model, CancellationToken.None);

        // When: UnRegisterAttendeeAsync is called
        Result<Unit> result = await Adapter.UnRegisterAttendeeAsync(new UnRegisterAttendeeModel()
        {
            MeetingID = scheduleMeetingAsync.Value.EventID,
            RegistrantID = registerResult.Value.RegistrantID
        }, CancellationToken.None);

        // Then: result is success
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");

        // And: Delete meeting  
        await Adapter.DeleteMetingAsync(scheduleMeetingAsync.Value.EventID, CancellationToken.None);
    }
}

