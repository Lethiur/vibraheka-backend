using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Admin.Commands.CreateTherapist;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController(IMediator mediator)
{
    /// <summary>
    /// Handles the creation of a new therapist by processing the provided command data.
    /// </summary>
    /// <param name="command">An instance of <c>CreateTherapistCommand</c> containing the email and name of the therapist to be created.</param>
    /// <returns>
    /// An <c>IActionResult</c> representing the HTTP response. If the operation is successful, returns a 200 OK response
    /// with the created therapist's identifier. If the operation fails, returns a 400 Bad Request with the error details
    /// or a 401 Unauthorized if the user lacks appropriate authorization.
    /// </returns>
    [HttpPost("addTherapist")]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> CreateTherapist([FromBody] CreateTherapistCommand command)
    {
        Result<string> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            switch (result.Error)
            {
                case UserException.NotAuthorized:
                    return new UnauthorizedResult();
                default:
                    return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
            }
        }
        
        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }
}
