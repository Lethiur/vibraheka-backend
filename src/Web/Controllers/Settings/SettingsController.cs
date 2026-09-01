using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Settings.Queries.GetTemplateForAction;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Settings.Controllers;

namespace VibraHeka.Web.Controllers.Settings;

public partial class SettingsController(IMediator mediator, ILogger<SettingsController> Logger, SettingsMapper mapper) : ISettingsController
{
    /// <summary>
    /// Updates the currently active template with the specified changes.
    /// Supports partial updates to the template configuration.
    /// </summary>
    /// <returns>Returns a status code indicating the result of the operation. A 200 status code indicates success, while a 400 status code indicates a bad request.</returns>
    public override async Task<IActionResult> UpdateAdminSettings(UpdateTemplateForActionRequest body,
        CancellationToken cancellationToken = default)
    {
        LogChangingTemplateForActionActionTypeUsingTemplateTemplateId(body.ActionType, body.TemplateId);

        Result<Unit> send = await mediator.Send(mapper.ToCommand(body), cancellationToken);

        if (send.IsFailure)
        {
            Logger.LogError("Failed to execute Change Template For Action because {Error}", send.Error);
            return BadRequest(new BadRequestResponse { ErrorCode = send.Error });
        }
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves all email templates.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of email templates if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    public override async Task<ActionResult<GetTemplateResponse>> GetAdminSettings(CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<TemplateForActionEntity>> result = await mediator.Send(new GetTemplatesForActionQuery(), cancellationToken);
        return Ok(new GetTemplateResponse() {TemplateList = [.. result.Value.Select(mapper.ToDto)] });
    }


    [LoggerMessage(LogLevel.Error, "Failed to get all templates for actions because {Error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<SettingsController> logger, string Error);
    
    [LoggerMessage(LogLevel.Information, "Changing template for action {ActionType} using template {TemplateID}")]
    partial void LogChangingTemplateForActionActionTypeUsingTemplateTemplateId(ActionType actionType, Guid templateID);
}
