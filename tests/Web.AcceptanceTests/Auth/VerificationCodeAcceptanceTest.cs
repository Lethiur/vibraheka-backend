using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Users.Queries.GetCode;
using VibraHeka.Domain.Entities;
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
        VerificationCodeEntity? content = responseEntity.GetContentAs<VerificationCodeEntity>();
        Assert.That(content, Is.Not.Null);
        Assert.That(content!.Code, Is.Not.Null.And.Not.Empty);
        Assert.That(content.UserName, Is.EqualTo(email));
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
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.Success, Is.False);
        Assert.That(responseEntity.ErrorCode, Does.Contain("No codes found"));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("invalid-email")]
    public async Task ShouldReturnBadRequestWhenRequestingCodeWithInvalidEmail(string invalidEmail)
    {
        // Given: a request with invalid email format.
        GetCodeQuery query = new(invalidEmail);

        // When: requesting verification code.
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/verification-code", query);

        // Then: endpoint returns validation error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.InvalidEmail));
    }
}
