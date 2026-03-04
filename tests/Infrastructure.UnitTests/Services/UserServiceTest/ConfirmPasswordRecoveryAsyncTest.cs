using System.Data;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest;

[TestFixture]
public class ConfirmPasswordRecoveryAsyncTest : GenericUserServiceTest
{
    [Test]
    public async Task ShouldConfirmPasswordRecovery()
    {
        // Given: Some mocking
        _cognitoMock.Setup(provider => provider.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>()))
            .ReturnsAsync(new ConfirmForgotPasswordResponse());
        
        // When: Service is invoked
        Result<Unit> forgotPasswordConfirmResult =
            await _service.ConfirmPasswordRecoveryAsync("a", "b", "c", CancellationToken.None);
        
        // Then: Should return success
        Assert.That(forgotPasswordConfirmResult.IsSuccess, Is.True);
        
        // And: Mock should have been invoked once
        _cognitoMock.Verify(provider => provider.ConfirmForgotPasswordAsync(It.Is<ConfirmForgotPasswordRequest>(request =>  request.Username == "a" && 
                request.ConfirmationCode == "b" &&
                request.Password == "c" &&
                request.ClientId == _configMock.ClientId
        )), Times.Once);
    }

    [Test]
    public async Task ShouldMapCognitoException()
    {
        // Given: Some mocking
        _cognitoMock.Setup(provider => provider.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>()))
            .ThrowsAsync(new UserNotFoundException("User not found"));
        
          
        // When: Service is invoked
        Result<Unit> forgotPasswordConfirmResult =
            await _service.ConfirmPasswordRecoveryAsync("a", "b", "c", CancellationToken.None);
        
        // Then: Should return success
        Assert.That(forgotPasswordConfirmResult.IsSuccess, Is.False);
        
        // And: Error should be mapped
        Assert.That(forgotPasswordConfirmResult.Error, Is.EqualTo(UserErrors.UserNotFound));
        
        // And: Mock should have been invoked once
        _cognitoMock.Verify(provider => provider.ConfirmForgotPasswordAsync(It.Is<ConfirmForgotPasswordRequest>(request =>  request.Username == "a" && 
            request.ConfirmationCode == "b" &&
            request.Password == "c" &&
            request.ClientId == _configMock.ClientId
        )), Times.Once);
    }

    [Test]
    public async Task ShouldMapUnknownError()
    {
        // Given: Some mocking
        _cognitoMock.Setup(provider => provider.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>()))
            .ThrowsAsync(new DataException("User not found"));
        
          
        // When: Service is invoked
        Result<Unit> forgotPasswordConfirmResult =
            await _service.ConfirmPasswordRecoveryAsync("a", "b", "c", CancellationToken.None);
        
        // Then: Should return success
        Assert.That(forgotPasswordConfirmResult.IsSuccess, Is.False);
        
        // And: Error should be mapped
        Assert.That(forgotPasswordConfirmResult.Error, Is.EqualTo(UserErrors.UnexpectedError));
        
        // And: Mock should have been invoked once
        _cognitoMock.Verify(provider => provider.ConfirmForgotPasswordAsync(It.Is<ConfirmForgotPasswordRequest>(request =>  request.Username == "a" && 
            request.ConfirmationCode == "b" &&
            request.Password == "c" &&
            request.ClientId == _configMock.ClientId
        )), Times.Once);
    }
}
