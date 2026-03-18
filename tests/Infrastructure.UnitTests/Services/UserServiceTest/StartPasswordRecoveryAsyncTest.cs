using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;


namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class StartPasswordRecoveryAsyncTest : GenericUserServiceTest
{

    [Test]
    public async Task ShouldReturnSuccessWhenEverythingGoesGood()
    {
        // Given: Some mocking
        _cognitoMock.Setup(provider => provider.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>()))
            .ReturnsAsync(new ForgotPasswordResponse());
        
        // When: Service is invoked
        Result<Unit> startPasswordRecoveryAsync = await _service.StartPasswordRecoveryAsync("email");
        
        // Then: Should return success
        Assert.That(startPasswordRecoveryAsync.IsSuccess, Is.True);
    }

    [Test]
    public async Task ShouldMapTheCognitoError()
    {
        // Given: Cognito throws an error
        _cognitoMock.Setup(provider => provider.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>()))
            .ThrowsAsync(new UserNotFoundException("User not found"));
        
        // When: Service is invoked
        Result<Unit> startPasswordRecoveryAsync = await _service.StartPasswordRecoveryAsync("email");
        
        // Then: The result should be failure
        Assert.That(startPasswordRecoveryAsync.IsFailure, Is.True);
        
        // And: The error should be the mapped one
        Assert.That(startPasswordRecoveryAsync.Error, Is.EqualTo(UserErrors.UserNotFound));
    }
}
