using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Application.Settings.Queries.GetTemplateForAction;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;

[ApiController]
[Route("api/v1/settings")]
public partial class SettingsController(IMediator mediator, ILogger<SettingsController> Logger)
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
    
    /// <summary>
    /// Retrieves all email templates.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of email templates if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    [HttpGet("all-templates")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTemplates()
    {
        Result<IEnumerable<TemplateForActionEntity>> result = await mediator.Send(new GetTemplatesForActionQuery());
        
        if (result.IsFailure)
        {
            LogFailedToGetAllTemplatesBecauseError(Logger, result.Error);
            if (result.Error == UserErrors.NotAuthorized)
            {
                return new UnauthorizedResult();
            }
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }
        
        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [LoggerMessage(LogLevel.Error, "Failed to get all templates for actions because {Error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<SettingsController> logger, string Error);
}
