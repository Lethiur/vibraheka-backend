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
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Application.Common.Models.Results;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace VibraHeka.Infrastructure.Services;

public class CognitoService(IConfiguration config, ILogger<CognitoService> logger) : ICognitoService
{
    private readonly AmazonCognitoIdentityProviderClient _client = CreateClient(config);
    private readonly string _userPoolId = config["Cognito:UserPoolId"] ?? "";
    private readonly string _clientId = config["Cognito:ClientId"] ?? "";


    /// <summary>
    /// Creates an instance of the Amazon Cognito Identity Provider client configured with the specified AWS region and profile.
    /// </summary>
    /// <param name="config">The application configuration containing AWS settings, such as region and profile name.</param>
    /// <returns>An instance of <see cref="AmazonCognitoIdentityProviderClient"/> initialized with the appropriate AWS credentials and region.</returns>
    private static AmazonCognitoIdentityProviderClient CreateClient(IConfiguration config)
    {
        RegionEndpoint? region = RegionEndpoint.GetBySystemName(config["AWS:Region"] ?? "eu-west-1");
        string? profileName = config["AWS:Profile"];

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

        return new AmazonCognitoIdentityProviderClient(new AmazonCognitoIdentityProviderConfig 
        { 
            RegionEndpoint = region 
        });
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
        catch (NotAuthorizedException)
        {
            return Result.Failure<AuthenticationResult>(UserException.InvalidPassword);
        }
        catch (UserNotFoundException)
        {
            return Result.Failure<AuthenticationResult>(UserException.UserNotFound);
        }
        catch (UserNotConfirmedException)
        {
            return Result.Failure<AuthenticationResult>(UserException.UserNotConfirmed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error authenticating user {Email}", email);
            return Result.Failure<AuthenticationResult>(UserException.UnexpectedError);
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
