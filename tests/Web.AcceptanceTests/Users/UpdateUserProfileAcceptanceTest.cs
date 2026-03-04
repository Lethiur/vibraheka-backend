using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results.User;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class UpdateUserProfileAcceptanceTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsNotAuthenticated()
    {
        // Given: a request payload without authenticated context.
        UserDTO payload = new()
        {
            Id = Guid.NewGuid().ToString(),
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
        (string userId, string email) = await AuthenticateAsConfirmedUser();
        UserDTO payload = new()
        {
            Id = userId,
            Email = email,
            FirstName = "UpdatedName",
            MiddleName = "UpdatedMiddle",
            LastName = "UpdatedLast",
            Bio = "Updated bio",
            PhoneNumber = "+34911111222"
        };

        // When: calling the update profile endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: endpoint returns successful update.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.True);

        // And: fetching profile reflects the updated values.
        HttpResponseMessage getProfileResponse = await Client.GetAsync($"/api/v1/users/{userId}");
        Assert.That(getProfileResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity profileEntity = await getProfileResponse.GetAsResponseEntityAndContentAs<UserDTO>();
        UserDTO? profile = profileEntity.GetContentAs<UserDTO>();
        Assert.That(profile, Is.Not.Null);
        Assert.That(profile!.FirstName, Is.EqualTo(payload.FirstName));
        Assert.That(profile.MiddleName, Is.EqualTo(payload.MiddleName));
        Assert.That(profile.LastName, Is.EqualTo(payload.LastName));
        Assert.That(profile.Bio, Is.EqualTo(payload.Bio));
        Assert.That(profile.PhoneNumber, Is.EqualTo(payload.PhoneNumber));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenPayloadIsInvalid()
    {
        // Given: an authenticated user with invalid payload format.
        await AuthenticateAsConfirmedUser();
        UserDTO payload = new()
        {
            Id = "not-a-guid",
            Email = "invalid-email"
        };

        // When: calling the update profile endpoint.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: validation fails and returns bad request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
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
            Id = secondUserId,
            Email = secondUserEmail,
            FirstName = "ShouldNot",
            LastName = "Update"
        };

        // When: the first user attempts to update second user's profile.
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then: endpoint rejects with not-authorized error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
        Assert.That(entity.ErrorCode, Is.EqualTo(UserErrors.NotAuthorized));
    }
}
