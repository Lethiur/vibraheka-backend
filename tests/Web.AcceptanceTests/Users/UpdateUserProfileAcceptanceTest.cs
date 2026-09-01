using System.Net;
using NUnit.Framework;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class UpdateUserProfileAcceptanceTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsNotAuthenticated()
    {
        // Given: a request payload without authenticated context.
        RemoveAuthHeader();
        UpdateProfileRequest payload = new() { Email = "test@example.com" };

        // When: calling the update profile endpoint.
        // Then: middleware rejects the request as unauthorized.
        await PerformCallAndExpectStatusCode(() => InvokeUpdateUserProfileEndpoint(payload),
            HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldUpdateUserProfileWhenAuthenticatedAndPayloadIsValid()
    {
        // Given: an authenticated user with a valid self-update payload.
        await AuthenticateAsNewUser();

        UpdateProfileRequest payload = new()
        {
            Email = "test@test.com",
            FirstName = "UpdatedName",
            MiddleName = "UpdatedMiddle",
            LastName = "UpdatedLast",
            Bio = "Updated bio",
            PhoneNumber = "+34911111222"
        };

        // When: calling the update profile endpoint.
        await PerformUpdateUserProfile(payload);

        // Then: fetching profile reflects the updated values.
        UserDTO updatedProfile = await PerformGetUserProfile(GetuserID());
        Assert.That(updatedProfile.FirstName, Is.EqualTo(payload.FirstName));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenPayloadIsInvalid()
    {
        // Given: an authenticated user with invalid payload format.
        await AuthenticateAsNewUser();
        UpdateProfileRequest payload = new() { Email = "invalid-email" };

        // When: calling the update profile endpoint.
        // Then: validation fails and returns bad request.
        await PerformCallAndExpectStatusCode(() => InvokeUpdateUserProfileEndpoint(payload),
            HttpStatusCode.BadRequest);
    }
}
