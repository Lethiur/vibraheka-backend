using System.ComponentModel;
using System.Net;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using VibraHeka.Domain.Events.Models;

namespace Infrastructure.Rest.Client.IntegrationTests.Zoom.AdaptersTest.MeetingAdapterTest;

[TestFixture]
public sealed class RegisterAttendeeAsyncTest : GenericMeetingAdapterTest
{
    [Test]
    [DisplayName("Should return success with registrant details when auth and Zoom register both succeed")]
    public async Task ShouldReturnSuccessWhenAuthAndRegistrationSucceed()
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

        // When: RegisterAttendeeAsync is called
        Result<RegisterAttendeeResult> result = await Adapter.RegisterAttendeeAsync(model, CancellationToken.None);

        // Then: result is success and contains registrant data mapped from the stub response
        Assert.That(result.IsSuccess, Is.True,
            $"Expected success but got failure with error: '{(result.IsFailure ? result.Error : "N/A")}'");
        Assert.That(result.Value.JoinURL, Is.Not.Null.And.Not.Empty,
            "Expected a non-empty join URL from the stub response");

        // And: Delete meeting  
        await Adapter.DeleteMetingAsync(scheduleMeetingAsync.Value.EventID, CancellationToken.None);
    }
}

