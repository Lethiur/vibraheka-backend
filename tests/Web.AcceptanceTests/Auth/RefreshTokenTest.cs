using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.AcceptanceTests.Utils;
using VibraHeka.Web.Authentication;

namespace VibraHeka.Web.AcceptanceTests.Auth;

[TestFixture]
public class RefreshTokenTest : GenericAuthAcceptanceTest
{
    [Test]
    [DisplayName("Should refresh access token for a confirmed authenticated user")]
    public async Task ShouldRefreshAccessTokenForAConfirmedAuthenticatedUser()
    {
        // Given: a registered and confirmed user with a valid refresh token from login.
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);
        AuthenticateUserResponse authenticationResult = await AuthenticateUser(email, ThePassword);

        RefreshTokenRequest command = new()
        {
            RefreshToken = authenticationResult.RefreshToken,
            Email = email
        };

        // When: requesting token refresh through the API.
        RefreshTokenResponse responseEntity =  await PerformCallAndRetrieveContent<RefreshTokenResponse>(() => InvokeRefreshTokenEndpoint(command));
        
        // Then: The tokens should be different
        Assert.That(responseEntity.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(responseEntity.AccessToken, Is.Not.EqualTo(authenticationResult.AccessToken));
    }
    
    [Test]
    [DisplayName("Should return bad request when refresh token is invalid")]
    public async Task ShouldReturnBadRequestWhenRefreshTokenIsInvalid()
    {
        // Given: a confirmed user and an invalid refresh token payload.
        string email = TheFaker.Internet.Email();
        await RegisterAndConfirmUser(email, ThePassword);

        RefreshTokenRequest command = new()
        {
            RefreshToken = "invalid-refresh-token",
            Email = email
        };

        // When: requesting token refresh.
        // Then: API should reject the request.
        await PerformCallAndExpectError(() => InvokeRefreshTokenEndpoint(command), UserErrors.NotAuthorized);
      
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
            Email = email!
        };

        // When: calling the refresh-token endpoint.
        await PerformCallAndExpectError(() => InvokeRefreshTokenEndpoint(command), expectedErrorCode);
    }
}
