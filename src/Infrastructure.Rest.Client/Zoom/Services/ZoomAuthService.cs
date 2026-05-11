using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Config;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Rest.Client.Zoom.Services;

public class ZoomAuthService(ZoomApiClient Client, IOptions<ZoomConfig> Config)
{
    private string? AuthToken;
    private DateTime TokenExpiration;


    public async Task<Result<string>> GetAuthTokenAsync(CancellationToken cancellationToken)
    {
        if (AuthToken != null && DateTime.UtcNow < TokenExpiration)
        {
            return AuthToken;
        }

        Result<ZoomAuthTokenResponse> authToken =
            await Client.GetAuthToken(Config.Value.ClientID, Config.Value.ClientSecret, Config.Value.AccountID,
                cancellationToken);

        if (authToken.IsFailure)
        {
            return Result.Failure<string>(authToken.Error);
        }

        ZoomAuthTokenResponse tokenResponse = authToken.Value;

        AuthToken = tokenResponse.AccessToken;
        TokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
        return AuthToken;
    }
}
