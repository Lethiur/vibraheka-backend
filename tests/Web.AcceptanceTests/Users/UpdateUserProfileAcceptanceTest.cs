using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;
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
        // Given
        UserDTO payload = new()
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com"
        };

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldUpdateUserProfileWhenAuthenticatedAndPayloadIsValid()
    {
        // Given
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

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.True);
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenPayloadIsInvalid()
    {
        // Given
        await AuthenticateAsConfirmedUser();
        UserDTO payload = new()
        {
            Id = "not-a-guid",
            Email = "invalid-email"
        };

        // When
        HttpResponseMessage response = await Client.PatchAsJsonAsync("/api/v1/users/update-profile", payload);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        ResponseEntity entity = await response.GetAsResponseEntity();
        Assert.That(entity.Success, Is.False);
    }
}

