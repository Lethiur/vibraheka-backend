using System.Data;
using System.IdentityModel.Tokens.Jwt;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Infrastructure.Entities;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace VibraHeka.Infrastructure.Services;

public class UserService(AWSConfig config, ILogger<UserService> logger) : IUserService
{
    protected IAmazonCognitoIdentityProvider _client = CreateClient(config);
    private readonly string _userPoolId = config.UserPoolId;
    private readonly string _clientId = config.ClientId;


    /// <summary>
    /// Creates an instance of the Amazon Cognito Identity Provider client configured with the specified AWS region and profile.
    /// </summary>
    /// <param name="config">The application configuration containing AWS settings, such as region and profile name.</param>
    /// <returns>An instance of <see cref="AmazonCognitoIdentityProviderClient"/> initialized with the appropriate AWS credentials and region.</returns>
    private static AmazonCognitoIdentityProviderClient CreateClient(AWSConfig config)
    {
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(config.Location);
        string? profileName = config.Profile;

        if (!string.IsNullOrEmpty(profileName))
        {
            CredentialProfileStoreChain chain = new CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials(profileName, out AWSCredentials? credentials))
            {
                return new AmazonCognitoIdentityProviderClient(credentials, new AmazonCognitoIdentityProviderConfig 
                { 
                    RegionEndpoint = region 
                });
            }
        }

        throw new DataException("AWS profile is required");
    }
    /// <summary>
    /// Registers a new user in the system by creating an account with the provided credentials and user information.
    /// </summary>
    /// <param name="email">The email address of the user to register.</param>
    /// <param name="password">The password to set for the user's account.</param>
    /// <param name="fullName">The full name of the user to associate with the account.</param>
    /// <returns>A result containing the unique identifier of the registered user if the registration is successful, or an error in case of failure.</returns>
    public async Task<Result<string>> RegisterUserAsync(string email, string password, string fullName)
    {
        try
        {
            SignUpRequest request = new SignUpRequest
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

            SignUpResponse? response = await _client.SignUpAsync(request);
            logger.Log(LogLevel.Information, "User registered successfully: {UserSub}", response.UserSub);
            return response.UserSub;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while registering the user {Email}", email);
            return MapCognitoException<string>(ex);
        }
    }

    /// <summary>
    /// Authenticates a user by validating the provided email and password against the Cognito user pool.
    /// </summary>
    /// <param name="email">The email address of the user attempting to authenticate.</param>
    /// <param name="password">The password associated with the user's account.</param>
    /// <returns>A result containing an <see cref="AuthenticationResult"/> with the user's ID, access token, and refresh token upon successful authentication, or an error in case of failure.</returns>
    public async Task<Result<AuthenticationResult>> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            AdminInitiateAuthRequest request = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _clientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password }
                }
            };

            AdminInitiateAuthResponse? response = await _client.AdminInitiateAuthAsync(request);
            
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jsonToken = handler.ReadJwtToken(response.AuthenticationResult.IdToken);
            string? userId = jsonToken.Subject; // El claim 'sub' suele mapearse a .Subject

            return Result.Success(new AuthenticationResult(userId, response.AuthenticationResult.AccessToken,response.AuthenticationResult.RefreshToken ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while authenticating the user {Email}", email);
            return MapCognitoException<AuthenticationResult>(ex);
        }
    }

    /// <summary>
    /// Resends the verification code to the specified user's email address.
    /// </summary>
    /// <param name="email">The email address of the user to whom the verification code should be resent.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating the success or failure of the operation.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public async Task<Result<Unit>> ResendVerificationCodeAsync(string email)
    {
        ResendConfirmationCodeRequest request = new()
        {
            ClientId = _clientId, Username = email
        };

        try
        {
            ResendConfirmationCodeResponse resendConfirmationCodeResponse =
                await _client.ResendConfirmationCodeAsync(request);
            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while resending the verification code for user {Email}", email);
            return MapCognitoException<Unit>(ex);
        }
    }

    /// <summary>
    /// Retrieves the unique user identifier (User ID) associated with the specified email address from the Cognito user pool.
    /// </summary>
    /// <param name="email">The email address of the user whose User ID is to be retrieved.</param>
    /// <returns>A <see cref="Result{T}"/> containing the User ID if the operation is successful; otherwise, an error result.</returns>
    public async Task<Result<string>> GetUserID(string email)
    {
        try
        {
            AdminGetUserRequest request = new AdminGetUserRequest()
            {
                UserPoolId = _userPoolId,
                Username = email
            };

            AdminGetUserResponse response = await _client.AdminGetUserAsync(request);
            return Result.Success(response.UserAttributes.First(attr => attr.Name == "sub").Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while requesting the new verification code for user with email {Email}", email);
            return MapCognitoException<string>(ex);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while error confirming user {Email}", email);
            return MapCognitoException<Unit>(ex);
        }
    }

    /// <summary>
    /// Maps exceptions thrown by the Amazon Cognito Identity Provider to corresponding application-specific error identifiers.
    /// </summary>
    /// <typeparam name="T">The type of the result that the operation will return in case of success.</typeparam>
    /// <param name="ex">The exception thrown during the execution of an operation in the Cognito service.</param>
    /// <returns>A <see cref="Result{T}"/> containing the appropriate error mapped from the exception, or a fallback error for unexpected exceptions.</returns>
    private Result<T> MapCognitoException<T>(Exception ex)
    {
        // Map common Cognito exceptions to your domain/application error strings.
        // Add/remove cases as you discover them.
        return ex switch
        {
            UsernameExistsException => Result.Failure<T>(UserErrors.UserAlreadyExist),
            InvalidPasswordException => Result.Failure<T>(UserErrors.InvalidPassword),
            InvalidParameterException => Result.Failure<T>(UserErrors.InvalidForm),

            NotAuthorizedException => Result.Failure<T>(UserErrors.NotAuthorized),
            UserNotFoundException => Result.Failure<T>(UserErrors.UserNotFound),
            UserNotConfirmedException => Result.Failure<T>(UserErrors.UserNotConfirmed),

            CodeMismatchException => Result.Failure<T>(UserErrors.WrongVerificationCode),
            ExpiredCodeException => Result.Failure<T>(UserErrors.ExpiredCode),

            TooManyFailedAttemptsException => Result.Failure<T>(UserErrors.TooManyAttempts),
            LimitExceededException => Result.Failure<T>(UserErrors.LimitExceeded),

            // Fallback: unexpected
            _ => Result.Failure<T>(UserErrors.UnexpectedError)
        };
    }
}
