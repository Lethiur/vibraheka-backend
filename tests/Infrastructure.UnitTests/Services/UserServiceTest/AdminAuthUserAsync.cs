using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class AdminAuthUserAsync : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return AuthenticationResult with role when admin authenticates user")]
    public async Task ShouldReturnAuthenticationResultWithRoleWhenAdminAuthenticatesUser()
    {
        // Given
        const string email = "user@test.com";
        const string expectedUserId = "user-guid-123";
        const UserRole expectedRole = UserRole.Admin;
        
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([new Claim("sub", expectedUserId)]),
            Expires = DateTime.UtcNow.AddHours(1)
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        string? idToken = tokenHandler.WriteToken(token);

        AdminInitiateAuthResponse authResponse = new()
        {
            AuthenticationResult = new AuthenticationResultType
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                IdToken = idToken
            }
        };

        CognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        UserEntity userEntity = new() { Id = expectedUserId, Email = email, Role = expectedRole };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(expectedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(userEntity));

        // When
        Result<AuthenticationResult> result = await _service.AdminAuthUserAsync(email, CancellationToken.None);

        // Then
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.UserID, Is.EqualTo(expectedUserId));
        Assert.That(result.Value.AccessToken, Is.EqualTo("access-token"));
        Assert.That(result.Value.RefreshToken, Is.EqualTo("refresh-token"));
        Assert.That(result.Value.Role, Is.EqualTo(expectedRole));
    }

    [Test]
    [DisplayName("Should return failure when Cognito fails")]
    public async Task ShouldReturnFailureWhenCognitoFails()
    {
        // Given
        CognitoMock.Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserNotFoundException("User not found"));

        // When
        Result<AuthenticationResult> result = await _service.AdminAuthUserAsync("none@test.com", CancellationToken.None);

        // Then
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
}
