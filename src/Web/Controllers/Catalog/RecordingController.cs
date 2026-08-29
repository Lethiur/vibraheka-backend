using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
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
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.Controllers.Catalog;

[ApiController]
[Route("api/v1/catalog/recordings")]
public class RecordingController(IMediator mediator)
{
    /// <summary>
    /// Registers a new recording entry and returns a pre-signed S3 PUT URL.
    /// The client must PUT the video file directly to the returned <c>uploadUrl</c>.
    /// Only administrators can perform this action.
    /// </summary>
    /// <param name="request">Recording metadata (no binary payload).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Object containing <c>recordingId</c> and <c>uploadUrl</c>.</returns>
    [HttpPost]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadRecording(
        [FromBody] UploadRecordingRequest request,
        CancellationToken ct)
    {
        AdminAddRecordingCommand command = new(
            Name: request.Name,
            Description: request.Description,
            Tier: request.Tier,
            Price: request.Price,
            CurrencyCode: request.CurrencyCode,
            Type: request.Type);

        Result<AddRecordingResult> result = await mediator.Send(command, ct);

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
    /// Returns all available recordings.
    /// </summary>
    /// <returns>List of recordings.</returns>
    [HttpGet("all")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ResponseEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllRecordingsAdmin()
    {
        Result<IEnumerable<RecordingEntity>> result = await mediator.Send(new AdminGetRecordingsQuery());

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

    /// <summary>
    /// Deletes an existing recording including its file and metadata. Only administrators can perform this action.
    /// </summary>
    /// <param name="recordingId">The unique identifier of the recording to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 NoContent on success.</returns>
    [HttpDelete("{recordingId}")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecording(string recordingId, CancellationToken ct)
    {
        Result<Unit> result = await mediator.Send(new DeleteRecordingCommand(recordingId), ct);

        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return new NotFoundObjectResult(ResponseEntity.FromError(result.Error));

            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        return new NoContentResult();
    }

    [HttpPatch("deactivate")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateRecording([FromBody] ModifyProductVisibilityRequest request, CancellationToken ct)
    {
        AdminDeActivateProductCommand command = new(request.ProductType, request.ProductID);
        Result<Unit> result = await mediator.Send(command, ct);
        
        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return new NotFoundObjectResult(ResponseEntity.FromError(result.Error));

            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        return new NoContentResult();
    }
    
    [HttpPatch("activate")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRecording([FromBody] ModifyProductVisibilityRequest request, CancellationToken ct)
    {
        AdminActivateProductCommand command = new(request.ProductType, request.ProductID);
        Result<Unit> result = await mediator.Send(command, ct);
        
        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return new NotFoundObjectResult(ResponseEntity.FromError(result.Error));

            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        return new NoContentResult();
    }
}
