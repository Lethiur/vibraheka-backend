using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Catalog.Commands.AdminActivateProduct;
using VibraHeka.Application.Catalog.Commands.AdminDeactivateProduct;
using VibraHeka.Application.Events.Commands.AdminCreateEvent;
using VibraHeka.Application.Events.Models;
using VibraHeka.Application.Events.Queries.GetEvents;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Events.Entities;
using VibraHeka.Web.Entities;
using VibraHeka.Web.Mappers;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventController(IMediator mediator, CreateEventMapper mapper)
{
    /// <summary>
    /// Creates a new event based on the provided command.
    /// The method processes the request to create an event, returning either a success or error response.
    /// </summary>
    /// <param name="request">An instance of <see cref="CreateEventCommand"/> containing the details necessary to
    /// create the event, such as name, description, date, duration, timezone, and price.</param>
    /// <param name="cancellationToken">A token used to cancel the operation, if needed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a successful
    /// response with the created event details or a bad request response with error details.</returns>
    [HttpPut]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        Result<EventDto> result = await mediator.Send(mapper.ToCommand(request), cancellationToken);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [Authorize]
    [Produces("application/json")]
    [HttpGet("{from:datetime}/to/{to:datetime}")]
    public async Task<IActionResult> GetEvents(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        Result<List<EventEntity>> result = await mediator.Send(new GetEventsQuery(from, to), cancellationToken);

        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
    
    [HttpPatch("deactivate")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeActivateEvent([FromBody] ModifyProductVisibilityRequest request, CancellationToken ct)
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
    public async Task<IActionResult> ActivateEvent([FromBody] ModifyProductVisibilityRequest request, CancellationToken ct)
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
