using System.Net;
using System.Net.Http.Json;
using Bogus;
using NUnit.Framework;
using VibraHeka.Application.Users.Commands.RegisterUser;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class ResendConfirmationCodeAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    public async Task ShouldResendConfirmationCodeWhenUserIsRegistered()
    {
        // Given: A registered but not confirmed user
        Faker faker = new();
        string email = faker.Internet.Email();
        RegisterUserCommand registerCommand = new(email, "Password123@", "John Doe");
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);

        // When: Resending the confirmation code
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<string>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.EqualTo("Confirmation code resent successfully"));
    }

    [Test]
    public async Task ShouldReturnBadRequestWhenEmailDoesNotExist()
    {
        // Given: An email that is not registered
        string email = "nonexistent@example.com";

        // When: Resending the confirmation code
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/auth/resend-confirmation-code?email={email}");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
