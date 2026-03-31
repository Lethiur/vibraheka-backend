using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class RefreshTokenTest : GenericAcceptanceTest<VibraHekaProgram>
{
    [Test]
    [DisplayName("Should refresh access token for a confirmed authenticated user")]
    public async Task ShouldRefreshAccessTokenForAConfirmedAuthenticatedUser()
    {
        // Given: a registered and confirmed user with a valid refresh token from login.
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);

        RefreshTokenRequest command = new()
        {
            RefreshToken = authenticationResult.RefreshToken,
            Username = email
        };

        // When: requesting token refresh through the API.
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", command);

        // Then: endpoint should return a new access token.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<string>();
        string? refreshedAccessToken = responseEntity.GetContentAs<string>();
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(refreshedAccessToken, Is.Not.Null.And.Not.Empty);
        
        // And: The tokens should be different
        Assert.That(refreshedAccessToken, Is.Not.EqualTo(authenticationResult.AccessToken));
    }

    [Test]
    [DisplayName("Should return a different JWT than the one obtained during login when refresh succeeds")]
    public async Task ShouldReturnDifferentJwtThanTheOneObtainedDuringLoginWhenRefreshSucceeds()
    {
        // Given: a registered and confirmed user with tokens from a successful login.
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);
        AuthenticationResult authenticationResult = await AuthenticateUser(email, ThePassword);

        RefreshTokenRequest command = new()
        {
            RefreshToken = authenticationResult.RefreshToken,
            Username = email
        };

        // When: refreshing the session.
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", command);

        // Then: the returned JWT should not be the same access token produced by login.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<string>();
        string? refreshedAccessToken = responseEntity.GetContentAs<string>();
        Assert.That(refreshedAccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(refreshedAccessToken, Is.Not.EqualTo(authenticationResult.AccessToken));
    }

    [Test]
    [DisplayName("Should return bad request when refresh token is invalid")]
    public async Task ShouldReturnBadRequestWhenRefreshTokenIsInvalid()
    {
        // Given: a confirmed user and an invalid refresh token payload.
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(TheFaker.Person.FullName, email, ThePassword);

        RefreshTokenRequest command = new()
        {
            RefreshToken = "invalid-refresh-token",
            Username = email
        };

        // When: requesting token refresh.
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", command);

        // Then: API should reject the request.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [TestCase("", "user@test.com", UserErrors.InvalidForm, TestName = "Empty refresh token")]
    [TestCase(null, "user@test.com", UserErrors.InvalidForm, TestName = "Null refresh token")]
    [TestCase("valid-looking-refresh-token", "", UserErrors.InvalidEmail, TestName = "Empty email")]
    [TestCase("valid-looking-refresh-token", null, UserErrors.InvalidEmail, TestName = "Null email")]
    [TestCase("valid-looking-refresh-token", "invalid-email", UserErrors.InvalidEmail, TestName = "Invalid email format")]
    [DisplayName("Should return bad request when refresh payload is invalid")]
    public async Task ShouldReturnBadRequestWhenRefreshPayloadIsInvalid(
        string? refreshToken,
        string? email,
        string expectedErrorCode)
    {
        // Given: a malformed refresh-token request.
        RefreshTokenRequest command = new()
        {
            RefreshToken = refreshToken!,
            Username = email!
        };

        // When: calling the refresh-token endpoint.
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/auth/refresh-token", command);

        // Then: validator or service should reject it with a domain error.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ResponseEntity responseEntity = await response.GetAsResponseEntity();
        Assert.That(responseEntity.ErrorCode, Is.EqualTo(expectedErrorCode));
    }
}
