using System.ComponentModel;
using System.Net;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class RefreshTokenTest : GenericUserServiceTest
{
    [Test]
    [DisplayName("Should return access token when Cognito refresh succeeds and send the expected request")]
    public async Task ShouldReturnAccessTokenWhenCognitoRefreshSucceeds()
    {
        // Given: Using a mocked cognito client to return a proper token
        const string refreshToken = "mock-refresh-token";
        const string email = "user@test.com";
        const string accessToken = "new-access-token";
        CancellationToken cancellationToken = new();
        ConfigMock.ClientId = "client-id-123";

        CognitoMock
            .Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), cancellationToken))
            .ReturnsAsync(new InitiateAuthResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                AuthenticationResult = new AuthenticationResultType
                {
                    AccessToken = accessToken
                }
            });

        // When: The service is invoked
        Result<string> result = await _service.RefreshToken(refreshToken, email, cancellationToken);

        // Then: The result should be success and the access token should be returned
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(accessToken));
        CognitoMock.Verify(x => x.InitiateAuthAsync(
            It.Is<InitiateAuthRequest>(request =>
                request.AuthFlow == AuthFlowType.REFRESH_TOKEN_AUTH &&
                request.AuthParameters["REFRESH_TOKEN"] == refreshToken &&
                request.AuthParameters["USERNAME"] == email),
            cancellationToken), Times.Once);
    }

    [Test]
    [DisplayName("Should return UnexpectedError when Cognito refresh response is not OK")]
    public async Task ShouldReturnUnexpectedErrorWhenCognitoRefreshStatusIsNotOk()
    {
        // Given: A cognito client mocked to return a non 200 code
        const string refreshToken = "mock-refresh-token";
        const string email = "user@test.com";
        ConfigMock.ClientId = "client-id-123";

        CognitoMock
            .Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), CancellationToken.None))
            .ReturnsAsync(new InitiateAuthResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                AuthenticationResult = new AuthenticationResultType
                {
                    AccessToken = "ignored-token"
                }
            });

        // When: Service is invoked
        Result<string> result = await _service.RefreshToken(refreshToken, email, CancellationToken.None);

        // Then: The result should be failure and the exception should be unexpected error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
        CognitoMock.Verify(x => x.InitiateAuthAsync(
            It.Is<InitiateAuthRequest>(request =>
                request.AuthFlow == AuthFlowType.REFRESH_TOKEN_AUTH &&
                request.AuthParameters["REFRESH_TOKEN"] == refreshToken &&
                request.AuthParameters["USERNAME"] == email),
            CancellationToken.None), Times.Once);
    }

    [Test]
    [DisplayName("Should map NotAuthorizedException when Cognito rejects the refresh token")]
    public async Task ShouldMapNotAuthorizedExceptionWhenCognitoRejectsTheRefreshToken()
    {
        // Given: A cognito client mocked to not authorized exception
        const string refreshToken = "expired-refresh-token";
        const string email = "user@test.com";
        ConfigMock.ClientId = "client-id-123";

        CognitoMock
            .Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), CancellationToken.None))
            .ThrowsAsync(new NotAuthorizedException("Invalid refresh token"));

        // When: Service is invoked
        Result<string> result = await _service.RefreshToken(refreshToken, email, CancellationToken.None);

        // Then: The result should be failure and the exception should be not authorized
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.NotAuthorized));
        CognitoMock.Verify(x => x.InitiateAuthAsync(
            It.Is<InitiateAuthRequest>(request =>
                request.AuthFlow == AuthFlowType.REFRESH_TOKEN_AUTH &&
                request.AuthParameters["REFRESH_TOKEN"] == refreshToken &&
                request.AuthParameters["USERNAME"] == email),
            CancellationToken.None), Times.Once);
    }

    [Test]
    [DisplayName("Should map InvalidParameterException when Cognito receives invalid refresh input")]
    public async Task ShouldMapInvalidParameterExceptionWhenCognitoReceivesInvalidRefreshInput()
    {
        // Given: A cognito client mocked to invalid parameter exception
        const string refreshToken = "bad-refresh-token";
        const string email = "user@test.com";
        ConfigMock.ClientId = "client-id-123";

        CognitoMock
            .Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), CancellationToken.None))
            .ThrowsAsync(new InvalidParameterException("Invalid parameter"));

        // When: Service is invoked
        Result<string> result = await _service.RefreshToken(refreshToken, email, CancellationToken.None);

        // Then: The result should be failure and the exception should be invalid form
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidForm));
    }

    [Test]
    [DisplayName("Should map TooManyRequestsException when Cognito throttles refresh requests")]
    public async Task ShouldMapTooManyRequestsExceptionWhenCognitoThrottlesRefreshRequests()
    {
        // Given: A cognito client mocked to rate exceeded exception
        CognitoMock
            .Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), CancellationToken.None))
            .ThrowsAsync(new TooManyRequestsException("Rate exceeded"));

        // When: Service is invoked
        Result<string> result = await _service.RefreshToken("mock-refresh-token", "user@test.com", CancellationToken.None);

        // Then: The result should be failure and the exception should be limit exceeded
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.LimitExceeded));
    }

    [Test]
    [DisplayName("Should map unexpected exceptions when Cognito refresh fails unexpectedly")]
    public async Task ShouldMapUnexpectedExceptionsWhenCognitoRefreshFailsUnexpectedly()
    {
        // Given: A cognito client mocked to general exception
        CognitoMock
            .Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), CancellationToken.None))
            .ThrowsAsync(new Exception("AWS is down"));

        // When: Service is invoked
        Result<string> result = await _service.RefreshToken("mock-refresh-token", "user@test.com", CancellationToken.None);

        // Then: The result should be failure and the exception should be unexpected error
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.UnexpectedError));
    }
}
