using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Infrastructure.Services;

public class CognitoService(IConfiguration config) : ICognitoService
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
        catch (Exception)
        {
            // Handle unexpected errors
            return Result.Failure<string>(UserException.UnexpectedError);
        }
    }
}
