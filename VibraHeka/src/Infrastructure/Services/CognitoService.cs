using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Infrastructure.Services;

public class CognitoService : ICognitoService
{
    private readonly AmazonCognitoIdentityProviderClient _client;
    private readonly string _userPoolId;
    private readonly string _clientId;

    public CognitoService(IConfiguration config)
    {
        _client = new AmazonCognitoIdentityProviderClient();
        _userPoolId = config["Cognito:UserPoolId"] ?? "";
        _clientId = config["Cognito:ClientId"] ?? "";
    }

    public async Task<string> RegisterUserAsync(string email, string password, string fullName)
    {
        var request = new SignUpRequest
        {
            ClientId = _clientId,
            Username = email,
            Password = password,
            UserAttributes = new List<AttributeType>
            {
                new() {Name = "name", Value = fullName},
                new() {Name = "email", Value = email}
            }
        };

        var response = await _client.SignUpAsync(request);

        return response.UserSub;
    }
}
