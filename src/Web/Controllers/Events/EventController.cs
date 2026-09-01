using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Commands.AdminActivateProduct;
using VibraHeka.Application.Catalog.Commands.AdminDeactivateProduct;
using VibraHeka.Application.Events.Commands.AdminCreateEvent;
using VibraHeka.Domain.Catalog.Enums;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Web.Events;

namespace VibraHeka.Web.Controllers.Events;

/// <summary>
/// Controller for handling event-related operations.
/// Provides endpoints to create, retrieve, and modify events within the system.
/// </summary>
public class EventController(IMediator mediator, EventMapper mapper) : IEventsController
{
    /// <summary>
    /// Creates a new event based on the provided command.
    /// The method processes the request to create an event, returning either a success or error response.
    /// </summary>
    /// <param name="body">An instance of <see cref="AdminCreateEventCommand"/> containing the details necessary to
    /// create the event, such as name, description, date, duration, timezone, and price.</param>
    /// <param name="cancellationToken">A token used to cancel the operation, if needed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a successful
    /// response with the created event details or a bad request response with error details.</returns>
    public override async Task<ActionResult<CreateEventResponse>> CreateEvent( CreateEventRequest body,
        CancellationToken cancellationToken = default)
    {
        Result<string> result = await mediator.Send(mapper.ToCommand(body), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }

        return Ok(mapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Retrieves a list of events based on the provided request parameters.
    /// The method processes the request and returns a response containing event details or an error if the operation fails.
    /// </summary>
    /// <param name="body">An instance of <see cref="GetEventsRequest"/> containing the filters and parameters
    /// required to fetch the events, such as date range or other criteria.</param>
    /// <param name="cancellationToken">A token used to signal cancellation of the operation, if necessary.</param>
    /// <returns>A <see cref="Task{ActionResult}"/> that resolves to an <see cref="ActionResult{T}"/> containing
    /// a <see cref="GetEventsResponse"/> object with the list of events or a bad request response with error details.</returns>
    public override async Task<ActionResult<GetEventsResponse>> GetEvents(GetEventsRequest body,
        CancellationToken cancellationToken = default)
    {
        Result<List<EventEntity>> result = await mediator.Send(mapper.ToQuery(body), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }
        return Ok(new GetEventsResponse { Events = result.Value.Select(mapper.ToResponse).ToList() });
    }

    /// <summary>
    /// Updates the visibility status of an event.
    /// This method processes the request to modify the visibility of an event based on the provided information.
    /// </summary>
    /// <param name="body">An instance of <see cref="ModifyEventVisibilityRequest"/> containing the event ID and the desired visibility status.</param>
    /// <param name="cancellationToken">A token used to cancel the operation, if required.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the operation. Returns <see cref="NoContentResult"/> if the update is successful,
    /// <see cref="NotFoundResult"/> if the event is not found, or a bad request response containing the error details in case of failure.</returns>
    public override async Task<IActionResult> ChangeEventVisibility(ModifyEventVisibilityRequest body,
        CancellationToken cancellationToken = default)
    {
        Result<Unit> result;
        if (body.Visible)
        {
            AdminActivateProductCommand command = new(ProductType.Event, body.EventId.ToString());
            result = await mediator.Send(command, cancellationToken);
        }
        else
        {
            AdminDeActivateProductCommand command = new(ProductType.Event, body.EventId.ToString());
            result = await mediator.Send(command, cancellationToken);
        }
        
        if (result.IsFailure)
        {
            if (result.Error == RecordingErrors.NotFound)
                return NotFound();

            return BadRequest(new BadRequestResponse { ErrorCode = result.Error });
        }
        return NoContent();
    }
    
}
