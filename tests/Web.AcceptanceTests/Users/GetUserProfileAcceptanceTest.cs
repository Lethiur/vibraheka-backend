using NUnit.Framework;
using System.Net;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results.User;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Users;

[TestFixture]
public class GetUserProfileAcceptanceTest : GenericUserAcceptanceTest
{
    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsNotAuthenticated()
    {
        // Given
        string userId = Guid.NewGuid().ToString();

        // When
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/users/{userId}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnUserProfileWhenAuthenticated()
    {
        // Given
        (string userId, _) = await AuthenticateAsConfirmedUser();

        // When
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/users/{userId}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        ResponseEntity entity = await response.GetAsResponseEntityAndContentAs<UserDTO>();
        UserDTO? dto = entity.GetContentAs<UserDTO>();

        Assert.That(entity.Success, Is.True);
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(userId));
    }
}

