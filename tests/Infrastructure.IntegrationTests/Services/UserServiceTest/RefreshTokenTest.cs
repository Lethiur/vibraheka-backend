using System.ComponentModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.UserServiceTest;

[TestFixture]
public class RefreshTokenTest : GenericCognitoServiceTest
{
    private const string DefaultPassword = "ValidPassword123!";

    [Test]
    [DisplayName("Should refresh access token when refresh token is valid")]
    public async Task ShouldRefreshAccessTokenWhenRefreshTokenIsValid()
    {
        // Given: a registered and confirmed user with a valid authenticated session.
        string email = GenerateUniqueEmail("test-refresh-success@");
        await RegisterUser(email);
        string codeResult = await GetAndDecryptCode(email);
        Result<Unit> confirmResult = await UserService.ConfirmUserAsync(email, codeResult);
        Assert.That(confirmResult.IsSuccess, Is.True);

        Result<AuthenticationResult> authenticationResult = await UserService.AuthenticateUserAsync(email, DefaultPassword);
        Assert.That(authenticationResult.IsSuccess, Is.True);
        Assert.That(authenticationResult.Value.RefreshToken, Is.Not.Null.And.Not.Empty);

        // When: requesting a new access token using the refresh token.
        Result<string> refreshResult = await UserService.RefreshToken(
            authenticationResult.Value.RefreshToken,
            email,
            CancellationToken.None);

        // Then: the refresh operation should succeed and return an access token.
        Assert.That(refreshResult.IsSuccess, Is.True);
        Assert.That(refreshResult.Value, Is.Not.Null.And.Not.Empty);

        // And: the access token should be different from the original one.
        Assert.That(refreshResult.Value, Is.Not.EqualTo(authenticationResult.Value.AccessToken));
    }

    [Test]
    [DisplayName("Should return not authorized when refresh token is invalid")]
    public async Task ShouldReturnNotAuthorizedWhenRefreshTokenIsInvalid()
    {
        // Given: a registered and confirmed user.
        string email = GenerateUniqueEmail("test-refresh-invalid-token@");
        await RegisterUser(email);
        string codeResult = await GetAndDecryptCode(email);
        Result<Unit> confirmResult = await UserService.ConfirmUserAsync(email, codeResult);
        Assert.That(confirmResult.IsSuccess, Is.True);

        // When: attempting refresh with an invalid token.
        Result<string> refreshResult = await UserService.RefreshToken(
            "invalid-refresh-token",
            email,
            CancellationToken.None);

        // Then: Cognito should reject it as not authorized.
        Assert.That(refreshResult.IsFailure, Is.True);
        Assert.That(refreshResult.Error, Is.EqualTo(UserErrors.NotAuthorized));
    }

    [TestCase("", "user@test.com", TestName = "Empty refresh token")]
    [TestCase(null, "user@test.com", TestName = "Null refresh token")]
    [DisplayName("Should return invalid form when refresh token payload is invalid")]
    public async Task ShouldReturnInvalidFormWhenRefreshTokenPayloadIsInvalid(string? refreshToken, string? email)
    {
        // When: attempting refresh with invalid input.
        Result<string> refreshResult = await UserService.RefreshToken(refreshToken!, email!, CancellationToken.None);

        // Then: invalid payload should map to InvalidForm.
        Assert.That(refreshResult.IsFailure, Is.True);
        Assert.That(refreshResult.Error, Is.EqualTo(UserErrors.InvalidForm));
    }

    [TestCase("valid-looking-token", "", TestName = "Empty email")]
    [TestCase("valid-looking-token", null, TestName = "Null email")]
    [DisplayName("Should return not authorized when email payload is invalid for refresh")]
    public async Task ShouldReturnNotAuthorizedWhenEmailPayloadIsInvalid(string? refreshToken, string? email)
    {
        // When: attempting refresh with invalid email input.
        Result<string> refreshResult = await UserService.RefreshToken(refreshToken!, email!, CancellationToken.None);

        // Then: Cognito rejects the refresh request as unauthorized.
        Assert.That(refreshResult.IsFailure, Is.True);
        Assert.That(refreshResult.Error, Is.EqualTo(UserErrors.NotAuthorized));
    }
}
