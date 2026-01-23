using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.EmailTemplates.Commands.AddAttachment;
using VibraHeka.Application.EmailTemplates.Commands.CreateEmail;
using VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.Entities;

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


    /// <summary>
    /// Creates a new email template using the provided details in the request.
    /// </summary>
    /// <param name="request">
    /// An instance of <see cref="UploadEmailTemplateRequest"/> containing the name of the template
    /// and the file to be uploaded.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating success or failure of the operation.
    /// On success, returns a confirmation of the created template;
    /// otherwise, an appropriate error response.
    /// </returns>
    [HttpPut("create")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateNewEmailTemplate([FromForm] UploadEmailTemplateRequest request)
    {
        CreateEmailTemplateCommand createEmailTemplateCommand = new CreateEmailTemplateCommand(request.File.OpenReadStream(), request.TemplateName);
        Result<Unit> mediatrResponse = await mediator.Send(createEmailTemplateCommand);

        if (mediatrResponse.IsFailure)
        {
            LogFailedToCreateNewTemplateBecauseError(Logger, mediatrResponse.Error);
            if (mediatrResponse.Error == UserErrors.NotAuthorized)
                return new UnauthorizedResult();
            return new BadRequestObjectResult(ResponseEntity.FromError(mediatrResponse.Error));
        }

        LogSuccessfullyCreatedNewTemplateTemplatename(Logger, request.TemplateName);

        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }

    [HttpPut("add-attachment")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddAttachmentToTemplate([FromForm] UploadEmailTemplateAttachment request)
    {
        AddAttachmentCommand command =
            new AddAttachmentCommand(request.File.OpenReadStream(), request.TemplateID, request.AttachmentName);
        Result<Unit> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            LogFailedToAddAttachmentToTemplateWithIdTemplateidBecauseError(Logger, request.TemplateID, result.Error);
            if (result.Error == UserErrors.NotAuthorized)
            {
                return new UnauthorizedResult();
            }
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }

    [LoggerMessage(LogLevel.Error, "Failed to get all templates because {Error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<EmailTemplateController> logger, string Error);

    [LoggerMessage(LogLevel.Information, "Successfully created new template {TemplateName}")]
    static partial void LogSuccessfullyCreatedNewTemplateTemplatename(ILogger<EmailTemplateController> logger, string TemplateName);

    [LoggerMessage(LogLevel.Error, "Failed to create new template because {Error}")]
    static partial void LogFailedToCreateNewTemplateBecauseError(ILogger<EmailTemplateController> logger, string Error);

    [LoggerMessage(LogLevel.Error, "Failed to add attachment to template with ID: {TemplateID} because {Error}")]
    static partial void LogFailedToAddAttachmentToTemplateWithIdTemplateidBecauseError(ILogger<EmailTemplateController> logger, string TemplateID, string Error);
}
