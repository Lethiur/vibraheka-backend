using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class VerificationCodeAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [Description("Should retrieve verification code when valid request is sent")]
    public async Task ShouldRetrieveVerificationCode()
    {
        // Given: A registered user
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        await RegisterAndConfirmUser(username, email, ThePassword);

        // When: Getting the verification code (this is a helper endpoint for testing/dev usually)
        GetCodeQuery query = new(email);
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/verification-code", query);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<VerificationCodeEntity>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(responseEntity.Content, Is.Not.Null);
    }

    [Test]
    [Description("Should return BadRequest when user does not exist")]
    public async Task ShouldReturnBadRequestWhenUserDoesNotExist()
    {
        // Given: A non-existent email
        GetCodeQuery query = new("nonexistent@example.com");

        // When: Attempting to get the code
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/verification-code", query);

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
