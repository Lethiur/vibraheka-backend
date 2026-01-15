using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Entities;
using static VibraHeka.Application.Common.Exceptions.UserErrors;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/settings")]
public class SettingsController(IMediator mediator, ILogger<SettingsController> Logger)
{
    /// <summary>
    /// Updates the currently active template with the specified changes.
    /// Supports partial updates to the template configuration.
    /// </summary>
    /// <returns>Returns a status code indicating the result of the operation. A 200 status code indicates success, while a 400 status code indicates a bad request.</returns>
    [Authorize]
    [HttpPatch("ChangeTemplate")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeTemplate([FromBody][Required] ChangeTemplateForActionCommand command)
    {
        Result<Unit> send = await mediator.Send(command);

        if (send.IsFailure)
        {
            Logger.LogError("Failed to execute Change Template For Action because {Error}", send.Error);
            return send.Error switch
            {
                UserErrors.NotAuthorized => new UnauthorizedResult(),
                _ => new BadRequestObjectResult(ResponseEntity.FromError(send.Error))
            };
        }
        
        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }
}
