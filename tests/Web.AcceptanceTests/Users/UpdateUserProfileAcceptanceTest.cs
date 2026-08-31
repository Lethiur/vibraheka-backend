using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;
using VibraHeka.Web.Users;
using BadRequestResponse = VibraHeka.Web.Users.BadRequestResponse;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class UpdateUserProfileAcceptanceTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsNotAuthenticated()
    {
        // Given: a request payload without authenticated context.
        Client.DefaultRequestHeaders.Remove("Authorization");
        UpdateProfileRequest payload = new()
        {
            Email = "test@example.com"
        };

        // When: calling the update profile endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: middleware rejects the request as unauthorized.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldUpdateUserProfileWhenAuthenticatedAndPayloadIsValid()
    {
        // Given: an authenticated user with a valid self-update payload.
        AuthenticateUserResponse authenticateAsConfirmedUser = await AuthenticateAsConfirmedUser();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authenticateAsConfirmedUser.AccessToken);
        UpdateProfileRequest payload = new()
        {
            FirstName = "UpdatedName",
            MiddleName = "UpdatedMiddle",
            LastName = "UpdatedLast",
            Bio = "Updated bio",
            PhoneNumber = "+34911111222"
        };

        // When: calling the update profile endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: endpoint returns successful update.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        
        // And: fetching profile reflects the updated values.
        HttpResponseMessage getProfileResponse = await Client.GetAsync($"/api/v1/users/{jwtSecurityToken.Subject}");
        getProfileResponse.EnsureSuccessStatusCode();
        UserDTO updatedProfile = await getProfileResponse.ParseContentAsync<UserDTO>();
        
        Assert.That(updatedProfile.FirstName, Is.EqualTo(payload.FirstName));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenPayloadIsInvalid()
    {
        // Given: an authenticated user with invalid payload format.
        await AuthenticateAsConfirmedUser();
        UserDTO payload = new()
        {
            Id = Guid.Parse("not-a-guid"),
            Email = "invalid-email"
        };

        // When: calling the update profile endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: validation fails and returns bad request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(UserErrors.InvalidUserID ));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenAuthenticatedUserTriesToUpdateAnotherUserProfile()
    {
        // Given: two users and authentication as the first user only.
        _ = await AuthenticateAsConfirmedUser();
        string secondUserEmail = TheFaker.Internet.Email();
        string secondUserId = await RegisterAndConfirmUser(TheFaker.Person.FullName, secondUserEmail, ThePassword);

        UserDTO payload = new()
        {
            Id = Guid.Parse(secondUserId),
            Email = secondUserEmail,
            FirstName = "ShouldNot",
            LastName = "Update"
        };

        // When: the first user attempts to update second user's profile.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: endpoint rejects with not-authorized error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        BadRequestResponse entity = await response.ParseContentAsync<BadRequestResponse>();
        Assert.That(entity.ErrorCode, Is.EqualTo(UserErrors.NotAuthorized));
    }
}
