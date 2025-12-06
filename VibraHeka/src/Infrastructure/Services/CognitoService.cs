using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace VibraHeka.Infrastructure.Services;

public class CognitoService(IConfiguration config, ILogger<CognitoService> logger) : ICognitoService
{
    private readonly AmazonCognitoIdentityProviderClient _client = new();
    private readonly string _userPoolId = config["Cognito:UserPoolId"] ?? "";
    private readonly string _clientId = config["Cognito:ClientId"] ?? "";

    public async Task<Result<string>> RegisterUserAsync(string email, string password, string fullName)
    {
        try
        {
            var request = new SignUpRequest
            {
                ClientId = _clientId,
                Username = email,
                Password = password,
                UserAttributes =
                [
                    new AttributeType { Name = "name", Value = fullName },
                    new AttributeType { Name = "email", Value = email }
                ]
            };

            var response = await _client.SignUpAsync(request);
            logger.Log(LogLevel.Information, "User registered successfully: {UserSub}", response.UserSub);
            return response.UserSub;
        }
        catch (UsernameExistsException)
        {
            return  Result.Failure<string>(UserException.UserAlreadyExist);
        }
        catch (InvalidPasswordException)
        {
            // Handle invalid password
            return Result.Failure<string>(UserException.InvalidPassword);
        }
        catch (InvalidParameterException)
        {
            // Handle invalid form data
            return Result.Failure<string>(UserException.InvalidForm);
        }
        catch (Exception E)
        {
            logger.LogError(E, "Unexpected error registering user");
            // Handle unexpected errors
            return Result.Failure<string>(UserException.UnexpectedError);
        }
    }

    /// <summary>
    /// Confirms a user's registration by verifying the provided confirmation code.
    /// </summary>
    /// <param name="email">The email address of the user to confirm.</param>
    /// <param name="confirmationCode">The verification code sent to the user's email.</param>
    /// <returns>A result containing a success indicator or an error in the case of failure.</returns>
    public async Task<Result<Unit>> ConfirmUserAsync(string email, string confirmationCode)
    {
        try
        {
            ConfirmSignUpRequest request = new()
            {
                Username = email, ConfirmationCode = confirmationCode, ClientId = _clientId
            };

            ConfirmSignUpResponse confirmSignUpResponse = await _client.ConfirmSignUpAsync(request);
            logger.Log(LogLevel.Information, "User with {Email} confirmed successfully", email);
            return Result.Success(Unit.Value);
        }
        catch (CodeMismatchException ex)
        {
            logger.LogWarning("Invalid confirmation code for {Email}: {Error}", email, ex.Message);
            return Result.Failure<Unit>(UserException.WrongVerificationCode);
        }
        catch (ExpiredCodeException ex)
        {
            logger.LogWarning("Confirmation code expired for {Email}: {Error}", email, ex.Message);
            return Result.Failure<Unit>(UserException.ExpiredCode);
        }
        catch (NotAuthorizedException ex)
        {
            logger.LogWarning("Not authorized to confirm {Email}: {Error}", email, ex.Message);
            return Result.Failure<Unit>(UserException.NotAuthorized);
        }
        catch (UserNotFoundException ex)
        {
            logger.LogWarning("User not found {Email}: {Error}", email, ex.Message);
            return Result.Failure<Unit>(UserException.UserNotFound);
        }
        catch (TooManyFailedAttemptsException ex)
        {
            logger.LogWarning("Too many failed attempts for {Email}: {Error}", email, ex.Message);
            return Result.Failure<Unit>(UserException.TooManyAttempts);
        }
        catch (InvalidParameterException)
        {
            return Result.Failure<Unit>(UserException.InvalidForm);
        }
        
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error confirming user {Email}", email);
            return Result.Failure<Unit>(EAppException.UnknownError);
        }
    }
}
