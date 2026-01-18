using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Controllers;

/// <summary>
/// Controller responsible for handling operations related to email templates.
/// </summary>
[ApiController]
[Route("api/v1/email-templates")]
public partial class EmailTemplateController(IMediator mediator, ILogger<EmailTemplateController> Logger)
{
    /// <summary>
    /// Retrieves all email templates.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of email templates if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTemplates()
    {
        Result<IEnumerable<EmailEntity>> result = await mediator.Send(new GetAllEmailTemplatesQuery());
        
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

    [LoggerMessage(LogLevel.Error, "Failed to get all templates because {Error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<EmailTemplateController> logger, string Error);
}
