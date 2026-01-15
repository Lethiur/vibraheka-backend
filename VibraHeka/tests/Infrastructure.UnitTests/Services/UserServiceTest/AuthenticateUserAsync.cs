using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class AuthenticateUserAsync : GenericUserServiceTest
{
     [Test]
    [DisplayName("Should return AuthenticationResult when credentials are valid")]
    public async Task ShouldReturnAuthenticationResultWhenCredentialsAreValid()
    {
        // Given: A valid user and a mocked JWT token
        const string email = "user@test.com";
        const string password = "Password123!";
        const string expectedUserId = "user-guid-123";
        
        // Creamos un JWT real pero simple para que JwtSecurityTokenHandler pueda leerlo
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("sub", expectedUserId)]),
            Expires = DateTime.UtcNow.AddHours(1)
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        string? idToken = tokenHandler.WriteToken(token);

        AdminInitiateAuthResponse authResponse = new AdminInitiateAuthResponse
        {
            AuthenticationResult = new AuthenticationResultType
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                IdToken = idToken
            }
        };

        _cognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ReturnsAsync(authResponse);

        // When: Authenticating the user
        Result<AuthenticationResult> result = await _service.AuthenticateUserAsync(email, password);

        // Then: Should return success with correct user ID and tokens
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(expectedUserId));
        Assert.That(result.Value.AccessToken, Is.EqualTo("access-token"));
        Assert.That(result.Value.RefreshToken, Is.EqualTo("refresh-token"));
    }

    [Test]
    [DisplayName("Should return InvalidPassword error when Cognito returns NotAuthorized")]
    public async Task ShouldReturnInvalidPasswordWhenNotAuthorized()
    {
        // Given: Cognito throws NotAuthorizedException
        _cognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ThrowsAsync(new NotAuthorizedException("Invalid credentials"));

        // When: Authenticating
        Result<AuthenticationResult> result = await _service.AuthenticateUserAsync("user@test.com", "wrong-pass");

        // Then: Should return InvalidPassword domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidPassword));
    }

    [Test]
    [DisplayName("Should return UserNotFound error when email does not exist")]
    public async Task ShouldReturnUserNotFoundWhenCognitoThrowsUserNotFound()
    {
        // Given: Cognito throws UserNotFoundException
        _cognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        // When: Authenticating
        Result<AuthenticationResult> result = await _service.AuthenticateUserAsync("none@test.com", "any-pass");

        // Then: Should return UserNotFound domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }

    [Test]
    [DisplayName("Should return UserNotConfirmed error when account is not confirmed")]
    public async Task ShouldReturnUserNotConfirmedWhenAccountIsPending()
    {
        // Given: Cognito throws UserNotConfirmedException
        _cognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ThrowsAsync(new UserNotConfirmedException("User is not confirmed"));

        // When: Authenticating
        Result<AuthenticationResult> result = await _service.AuthenticateUserAsync("pending@test.com", "pass123");

        // Then: Should return UserNotConfirmed domain error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotConfirmed));
    }

    [Test]
    [DisplayName("Should return UnexpectedError and log it when a general exception occurs")]
    public async Task ShouldReturnUnexpectedErrorWhenGeneralExceptionOccurs()
    {
        // Given: A random exception
        _cognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ThrowsAsync(new Exception("Cognito is down"));

        // When: Authenticating
        Result<AuthenticationResult> result = await _service.AuthenticateUserAsync("user@test.com", "pass");

        // Then: Should fail with UnexpectedError
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }
}
