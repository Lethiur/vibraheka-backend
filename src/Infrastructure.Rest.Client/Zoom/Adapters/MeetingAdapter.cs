using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Zoom.Config;
using Infrastructure.Rest.Client.Zoom.Mappers;
using Infrastructure.Rest.Client.Zoom.Models;
using Infrastructure.Rest.Client.Zoom.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Events.Models;
using VibraHeka.Domain.Events.Ports.Out;

namespace Infrastructure.Rest.Client.Zoom.Adapters;

public class MeetingAdapter(
    ZoomAuthService zoomAuthService,
    ZoomApiClient Client,
    IOptions<ZoomConfig> Config,
    ZoomMeetingMapper Mapper,
    ILogger<MeetingAdapter> Logger) : IEventMeetingPort
{
    /// <summary>
    /// Schedules a new meeting using the provided event details.
    /// </summary>
    /// <param name="model">The model containing the details of the event to be scheduled.</param>
    /// <param name="cancellationToken">A token that allows the operation to be canceled.</param>
    /// <returns>A result containing the details of the created event or an error if the operation fails.</returns>
    public async Task<Result<CreateEventResult>> ScheduleMeetingAsync(CreateEventModel model,
        CancellationToken cancellationToken)
    {
        Result<string> token = await zoomAuthService.GetAuthTokenAsync(cancellationToken);
        if (token.IsFailure)
        {
            Logger.LogError("Failed to get Zoom auth token: {Error}", token.Error);
            return Result.Failure<CreateEventResult>(token.Error);
        }
        string authToken = token.Value;
        ZoomCreateMeetingRequest request = Mapper.ToZoomRequest(model);
        Result<ZoomCreateMeetingResponse> meetingAsync = await Client.CreateMeetingAsync(authToken, Config.Value.HostEmail, request, cancellationToken);

        if (meetingAsync.IsFailure)
        {
            Logger.LogError("Failed to create Zoom meeting: {Error}", meetingAsync.Error);
            return Result.Failure<CreateEventResult>(meetingAsync.Error);
        }
        return Mapper.CreateMeetingResponseToDomain(meetingAsync.Value);
    }

    /// <summary>
    /// Deletes an existing meeting using the specified meeting ID.
    /// </summary>
    /// <param name="meetingId">The unique identifier of the meeting to be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure of the operation.</returns>
    public async Task<Result<Unit>> DeleteMetingAsync(long meetingId, CancellationToken cancellationToken)
    {
        (_, bool isFailure, string? authToken, string? error) = await zoomAuthService.GetAuthTokenAsync(cancellationToken);
        if (isFailure)
        {
            Logger.LogError("Failed to get Zoom auth token: {Error}", error);
            return Result.Failure<Unit>(error);
        }

        return await Client.DeleteMeetingAsync(authToken, meetingId, cancellationToken);
    }

    /// <summary>
    /// Registers a new attendee for a meeting using the provided attendee details.
    /// </summary>
    /// <param name="model">The model containing the details of the attendee to be registered.</param>
    /// <param name="cancellationToken">A token that allows the operation to be canceled.</param>
    /// <returns>A result containing the registration details if successful, or an error if the registration fails.</returns>
    public async Task<Result<RegisterAttendeeResult>> RegisterAttendeeAsync(RegisterAttendeeModel model,
        CancellationToken cancellationToken)
    {
        (_, bool isFailure, string authToken, string error) = await zoomAuthService.GetAuthTokenAsync(cancellationToken);
        if (isFailure)
        {
            Logger.LogError("Failed to get Zoom auth token: {Error}", error);
            return Result.Failure<RegisterAttendeeResult>(error);
        }

        ZoomRegisterRegistrantRequest zoomRequest = Mapper.ToZoomRequest(model);
        Result<ZoomCreateRegistrantResposne> registerParticipantAsync = await Client.RegisterParticipantAsync(authToken, zoomRequest, cancellationToken);
        return registerParticipantAsync.Map(Mapper.ZoomRegisterRegistrantResponseToDomain);
    }

    /// <summary>
    /// Unregisters an attendee from a specified meeting using the provided details.
    /// </summary>
    /// <param name="model">The model containing the attendee and meeting details required for unregistration.</param>
    /// <param name="cancellationToken">A token that allows the operation to be canceled.</param>
    /// <returns>A result indicating the success of the unregistration operation or an error if the operation fails.</returns>
    public async Task<Result<Unit>> UnRegisterAttendeeAsync(UnRegisterAttendeeModel model,
        CancellationToken cancellationToken)
    {
        (_, bool isFailure, string? authToken, string? error) = await zoomAuthService.GetAuthTokenAsync(cancellationToken);
        if (isFailure)
        {
            Logger.LogError("Failed to get Zoom auth token: {Error}", error);
            return Result.Failure<Unit>(error);
        }

        return await Client.UnRegisterParticipantAsync(authToken, Mapper.ToZoomUnRegisterRegistrantRequest(model), cancellationToken);
    }
}
