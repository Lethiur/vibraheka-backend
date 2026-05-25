using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Errors;
using Infrastructure.Rest.Client.Zoom.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Rest.Client.Zoom;

public class ZoomApiClient(ILogger<ZoomApiClient> Logger, HttpClient Client)
{
    private const string ZoomApiUrl = "https://api.zoom.us/";
    private const string ZoomAuthUrl = $"{ZoomApiUrl}oauth/token?grant_type=account_credentials&account_id={{0}}";
    private const string ZoomCreateMeetingUrl = $"{ZoomApiUrl}v2/users/{{0}}/meetings";
    private const string ZoomMeetingUrl = $"{ZoomApiUrl}v2/meetings/{{0}}";
    private const string ZoomRegistrantUrl = $"{ZoomApiUrl}v2/meetings/{{0}}/registrants";
    private const string ZoomRegistrantManagementUrl = $"{ZoomRegistrantUrl}/{{1}}";

    /// <summary>
    /// Retrieves an authentication token from the Zoom API for accessing the account's resources.
    /// </summary>
    /// <param name="clientID">The client ID used for authentication.</param>
    /// <param name="clientSecret">The client secret used for authentication.</param>
    /// <param name="accountID">The account ID of the Zoom account for which the token is requested.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result object containing the Zoom authentication token response if successful, or an error if the operation fails.</returns>
    public async Task<Result<ZoomAuthTokenResponse>> GetAuthToken(string clientID, string clientSecret,
        string accountID, CancellationToken cancellationToken)
    {
        string basicAuth = GenerateAuthHeaderValue(clientID, clientSecret);
        string url = string.Format(ZoomAuthUrl, accountID);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        HttpResponseMessage response = await Client.PostAsync(url, null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogError("Failed to retrieve Zoom auth token: {StatusCode}", response.StatusCode);
            return Result.Failure<ZoomAuthTokenResponse>(ZoomErrors.FailedToRetrieveToken);
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            ZoomAuthTokenResponse? tokenResponse = JsonSerializer.Deserialize<ZoomAuthTokenResponse>(responseBody);
            if (tokenResponse == null)
            {
                Logger.LogError("Failed to deserialize Zoom auth token response: {Response}", responseBody);
                return Result.Failure<ZoomAuthTokenResponse>(ZoomErrors.FailedToRetrieveToken);
            }

            return tokenResponse;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to deserialize Zoom auth token response: {Response}", responseBody);
            return Result.Failure<ZoomAuthTokenResponse>(ZoomErrors.FailedToRetrieveToken);
        }

    }

    /// <summary>
    /// Creates a new Zoom meeting for the specified host using the provided request details.
    /// </summary>
    /// <param name="authToken">The authentication token used to authorize the request with the Zoom API.</param>
    /// <param name="hostEmail">The email address of the host for whom the meeting will be created.</param>
    /// <param name="request">A request object containing the details for the meeting to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result object containing the Zoom meeting creation response if successful, or an error if the operation fails.</returns>
    public async Task<Result<ZoomCreateMeetingResponse>> CreateMeetingAsync(string authToken,
        string hostEmail,
        ZoomCreateMeetingRequest request, CancellationToken cancellationToken)
    {
        string endpoint = string.Format(ZoomCreateMeetingUrl, Uri.EscapeDataString(hostEmail));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        HttpResponseMessage response = await Client.PostAsJsonAsync(endpoint, request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Logger.LogError("Failed to create Zoom meeting: {StatusCode}, error: {Error}", response.StatusCode, errorContent);
            return Result.Failure<ZoomCreateMeetingResponse>(ZoomErrors.FailedToCreateMeeting);
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        ZoomCreateMeetingResponse? createMeetingResponse =
            JsonSerializer.Deserialize<ZoomCreateMeetingResponse>(responseBody);
        if (createMeetingResponse == null)
        {
            Logger.LogError("Failed to deserialize Zoom meeting creation response: {Response}", responseBody);
            return Result.Failure<ZoomCreateMeetingResponse>(ZoomErrors.FailedToCreateMeeting);
        }

        return createMeetingResponse;
    }

    /// <summary>
    /// Deletes a scheduled meeting in the Zoom system based on the provided meeting ID.
    /// </summary>
    /// <param name="authToken">The authentication token required to authorize the operation.</param>
    /// <param name="meetingId">The unique identifier of the meeting to be deleted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result object indicating success or failure of the meeting deletion operation.</returns>
    public async Task<Result<Unit>> DeleteMeetingAsync(string authToken, long meetingId,
        CancellationToken cancellationToken)
    {
        string url = String.Format(ZoomMeetingUrl, meetingId);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        HttpResponseMessage response = await Client.DeleteAsync(url, cancellationToken);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            Logger.LogError("Failed to delete Zoom meeting: {StatusCode}", response.StatusCode);
            return Result.Failure<Unit>(ZoomErrors.FailedToDeleteMeeting);
        }

        return Result.Success(Unit.Value);
    }

    /// <summary>
    /// Registers a participant for a specified Zoom meeting.
    /// </summary>
    /// <param name="authToken">The authentication token required to access the Zoom API.</param>
    /// <param name="request">The registration request containing participant information and the target meeting ID.</param>
    /// <param name="cancellationToken">The token that allows the operation to be canceled.</param>
    /// <returns>A result object containing the registration response if the operation is successful, or an error if it fails.</returns>
    public async Task<Result<ZoomCreateRegistrantResposne>> RegisterParticipantAsync(string authToken,
        ZoomRegisterRegistrantRequest request, CancellationToken cancellationToken)
    {
        string url = string.Format(ZoomRegistrantUrl, request.MeetingID);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        HttpResponseMessage response = await Client.PostAsJsonAsync(url, request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            Logger.LogError("Failed to register Zoom participant: {StatusCode}", response.StatusCode);
            return Result.Failure<ZoomCreateRegistrantResposne>(ZoomErrors.FailedToRegisterParticipant);
        }

        ZoomCreateRegistrantResposne? registrant =
            await response.Content.ReadFromJsonAsync<ZoomCreateRegistrantResposne>(
                cancellationToken: cancellationToken);
        if (registrant == null)
        {
            Logger.LogError("Failed to deserialize Zoom registrant response: {Response}", await response.Content.ReadAsStringAsync(cancellationToken));
            return Result.Failure<ZoomCreateRegistrantResposne>(ZoomErrors.FailedToRegisterParticipant);
        }

        return registrant;
    }

    /// <summary>
    /// Removes a registrant from a specified Zoom meeting.
    /// </summary>
    /// <param name="authToken">The authorization token used to authenticate the API request.</param>
    /// <param name="request">The request object containing the meeting ID and registrant ID to identify the registrant to be removed.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A result object indicating success if the registrant is successfully removed, or an error if the operation fails.</returns>
    public async Task<Result<Unit>> UnRegisterParticipantAsync(string authToken,
        ZoomUnRegisterRegistrantRequest request,
        CancellationToken cancellationToken)
    {
        string url = string.Format(ZoomRegistrantManagementUrl, request.MeetingID, request.RegistrantID);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        HttpResponseMessage response = await Client.DeleteAsync(url, cancellationToken);
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            Logger.LogError("Failed to unregister Zoom participant: {StatusCode}", response.StatusCode);
            return Result.Failure<Unit>(ZoomErrors.FailedToUnregisterParticipant);
        }

        return Result.Success(Unit.Value);
    }


    /// <summary>
    /// Generates a Base64-encoded authentication header value for Zoom API.
    /// </summary>
    /// <param name="clientID">The client ID used for authentication.</param>
    /// <param name="clientSecret">The client secret used for authentication.</param>
    /// <returns>A Base64-encoded string containing the client ID and secret combined as "clientID:clientSecret".</returns>
    private static string GenerateAuthHeaderValue(string clientID, string clientSecret)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientID}:{clientSecret}"));
    }
}
