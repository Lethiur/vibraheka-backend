using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Commands.AdminActivateProduct;
using VibraHeka.Application.Catalog.Commands.AdminAddRecording;
using VibraHeka.Application.Catalog.Commands.AdminDeactivateProduct;
using VibraHeka.Application.Catalog.Commands.DeleteRecording;
using VibraHeka.Application.Catalog.Models;
using VibraHeka.Application.Catalog.Queries.AdminGetRecordings;
using VibraHeka.Application.Catalog.Queries.GetAllRecordings;
using VibraHeka.Application.Catalog.Queries.GetRecordingDownloadUrl;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Web.Catalog.Recordings.Controllers;

namespace VibraHeka.Web.Controllers.Catalog.Recordings;

/// <summary>
/// Controller for managing recordings in the catalog. Provides endpoints for creating, retrieving, updating, and deleting recordings.
/// </summary>
/// <param name="mediator">The mediator instance for handling commands and queries.</param>
/// <param name="mapper">The mapper instance for converting between domain models and DTOs.</param>
public class RecordingController(IMediator mediator, RecordingMapper mapper) : IRecordingController
{
    /// <summary>
    /// Registers a new recording entry and returns a pre-signed S3 PUT URL.
    /// The client must PUT the video file directly to the returned <c>uploadUrl</c>.
    /// Only administrators can perform this action.
    /// </summary>
    /// <param name="body">Recording metadata (no binary payload).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Object containing <c>recordingId</c> and <c>uploadUrl</c>.</returns>
    ///
    public override async Task<ActionResult<CreateRecordingResponse>> AdminCreateRecording(CreateRecordingRequest body,
        CancellationToken cancellationToken = default)
    {
        AdminAddRecordingCommand command = mapper.ToAdminCommand(body);
        Result<AddRecordingResult> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(mapper.ToResponse(result.Value));
    }
    
    /// <summary>
    /// Returns a list of all recordings in the catalog. Only administrators can perform this action.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of recordings.</returns>
    public override async Task<ActionResult<AdminGetRecordingResponse>> AdminGetRecordings(CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<RecordingEntity>> result = await mediator.Send(new AdminGetRecordingsQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }

        return Ok(new AdminGetRecordingResponse {
            Recordings = result.Value.Select(mapper.ToAdminResponse).ToList()
        });
    }
    
    /// <summary>
    /// Returns a list of all recordings in the catalog. This endpoint is publicly accessible and does not require authentication.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to track this request</param>
    /// <returns></returns>
    public override async Task<ActionResult<GetRecordingsResponse>> GetAllRecordings(CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<RecordingEntity>> result = await mediator.Send(new GetAllRecordingsQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }

        return Ok(new GetRecordingsResponse {
            Recordings = [.. result.Value.Select(mapper.ToResponse)]
        });
    }


    /// <summary>
    /// Returns a temporary download URL for the specified recording.
    /// </summary>
    /// <param name="recordingId">The unique identifier of the recording.</param>
    /// <param name="cancellationToken">The cancellation token to track this request</param>
    /// <returns>A pre-signed download URL valid for a limited time.</returns>
    public override async Task<ActionResult<GetRecordingDownloadUrlResponse>> GetRecordingDownloadUrl(Guid recordingId, CancellationToken cancellationToken = default)
    {
        Result<string> result =
            await mediator.Send(new GetRecordingDownloadUrlQuery(recordingId.ToString()), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return new NotFoundResult();

            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });

        }

        return new OkObjectResult(new GetRecordingDownloadUrlResponse { DownloadUrl = result.Value });
    }

    /// <summary>
    /// Deletes an existing recording including its file and metadata. Only administrators can perform this action.
    /// </summary>
    /// <param name="recordingId">The unique identifier of the recording to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 NoContent on success.</returns>
    public override async Task<IActionResult> AdminDeleteRecording(Guid recordingId, CancellationToken cancellationToken = default)
    {
        Result<Unit> result = await mediator.Send(new DeleteRecordingCommand(recordingId.ToString()), cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return NotFound();

            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }

        return NoContent();
    }
    
    /// <summary>
    /// Updates the visibility of a recording (activate or deactivate). Only administrators can perform this action.
    /// </summary>
    /// <param name="body">The request containing the recording ID and the desired visibility state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public override async Task<IActionResult> AdminUpdateRecording(ModifyRecordingVisibilityRequest body,
        CancellationToken cancellationToken = default)
    {
        Result<Unit> result;
        if (body.Visible)
        {
            AdminActivateProductCommand command = new(ProductType.DigitalRecording, body.RecordingId.ToString());
            result = await mediator.Send(command, cancellationToken);
        }
        else
        {
            AdminDeActivateProductCommand command = new(ProductType.DigitalRecording, body.RecordingId.ToString());
            result = await mediator.Send(command, cancellationToken);
        }
        
        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return new NotFoundResult();

            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });

        }

        return NoContent();
    }
}
