using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Recordings.Commnads.AdminAddRecording;
using VibraHeka.Application.Recordings.Queries.GetAllRecordings;
using VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/recordings")]
public class RecordingController(IMediator mediator)
{
    /// <summary>
    /// Uploads a new video recording. Only administrators can perform this action.
    /// </summary>
    /// <param name="request">The video file to upload request.</param>
    /// <returns>The ID of the newly created recording on success.</returns>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadRecording(
        [FromForm] UploadRecordingRequest request)
    {

        AdminAddRecordingCommand command = new(
            Name: request.Name,
            Description: request.Description,
            Type: request.Type,
            FileStream: request.File.OpenReadStream(),
            FileName: request.File.FileName);

        Result<string> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    /// <summary>
    /// Returns all available recordings.
    /// </summary>
    /// <returns>List of recordings.</returns>
    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllRecordings()
    {
        Result<IEnumerable<RecordingDto>> result = await mediator.Send(new GetAllRecordingsQuery());

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    /// <summary>
    /// Returns a temporary download URL for the specified recording.
    /// </summary>
    /// <param name="recordingId">The unique identifier of the recording.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A pre-signed download URL valid for a limited time.</returns>
    [HttpGet("{recordingId}/download-url")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDownloadUrl(string recordingId, CancellationToken ct)
    {
        Result<RecordingDownloadUrlDto> result =
            await mediator.Send(new GetRecordingDownloadUrlQuery(recordingId), ct);

        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return new NotFoundObjectResult(ResponseEntity.FromError(result.Error));

            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
}
