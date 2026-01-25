using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.EmailTemplates.Commands.AddAttachment;
using VibraHeka.Application.EmailTemplates.Commands.CreateEmail;
using VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;
using VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;
using VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;
using VibraHeka.Application.EmailTemplates.Queries.GetTemplateContent;
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

        CreateEmailTemplateCommand createEmailTemplateCommand =
            new CreateEmailTemplateCommand(request.File.OpenReadStream(), request.TemplateName);
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

    /// <summary>
    /// Adds a new attachment to an existing email template.
    /// </summary>
    /// <param name="request">
    /// The attachment details, including file, template ID, and attachment name.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating success with an empty response body or an appropriate error response.
    /// </returns>
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

    /// <summary>
    /// Updates the name of an existing email template.
    /// </summary>
    /// <param name="request">
    /// An <see cref="EditTemplateNameRequest"/> containing the template ID and the new name to be assigned to the template.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating whether the operation was successful. Returns an error response if the operation fails,
    /// including an unauthorized result if the caller lacks permissions.
    /// </returns>
    [HttpPatch("change-name")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> ChangeTemplateName([FromBody] EditTemplateNameRequest request)
    {
        EditTemplateNameCommand command = new(request.TemplateID, request.NewTemplateName);
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            Logger.LogError("Failed to change template name for template with ID '{TemplateID}' because {Error}",
                request.TemplateID, result.Error);
            if (result.Error == UserErrors.NotAuthorized)
            {
                return new UnauthorizedResult();
            }

            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }

    /// <summary>
    /// Updates the contents of an existing email template based on the provided request data.
    /// </summary>
    /// <param name="request">
    /// An <see cref="EditTemplateNameRequest"/> containing the template ID and the new content details.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the update operation. Returns a success response if the update is successful,
    /// a bad request response if the operation fails due to input errors, or an unauthorized response if the user lacks necessary permissions.
    /// </returns>
    [HttpPatch("change-contents")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChangeTemplateContents([FromForm] EditTemplateContentRequest request)
    {
        UpdateTemplateContentCommand command = new(request.TemplateID, request.TemplateFile.OpenReadStream());
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            LogFailedToChangeTemplateNameForTemplateWithIdTemplateidBecauseError(Logger, request.TemplateID,
                result.Error);
            if (result.Error == UserErrors.NotAuthorized)
            {
                return new UnauthorizedResult();
            }

            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(""));
    }

    /// <summary>
    /// Retrieves the download URL for a specified email template.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the download URL if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    [HttpGet("url")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTemplateUrl([FromQuery(Name = "TemplateID")] string templateID)
    {
        GetEmailTemplateURLQuery query = new(templateID);
        Result<string> result = await mediator.Send(query);
        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    /// <summary>
    /// Retrieves the contents of a specified template.
    /// </summary>
    /// <param name="templateID">
    /// The unique identifier of the template whose contents are to be retrieved.
    /// </param>
    /// <returns>
    /// A string representing the contents of the specified template if found,
    /// or null if the template does not exist.
    /// </returns>
    [HttpGet("contents")]
    [Authorize]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<IActionResult> GetTemplateContents([FromQuery(Name = "templateID")] string templateID)
    {
        GetEmailTemplateContentQuery query = new(templateID);
        Result<string> result = await mediator.Send(query);
        if (result.IsFailure)
        {
            return new BadRequestObjectResult(ResponseEntity.FromError(result.Error));
        }

        return new OkObjectResult(ResponseEntity.FromSuccess(result.Value));
    }

    [LoggerMessage(LogLevel.Error, "Failed to get all templates because {Error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<EmailTemplateController> logger, string Error);

    [LoggerMessage(LogLevel.Information, "Successfully created new template {TemplateName}")]
    static partial void LogSuccessfullyCreatedNewTemplateTemplatename(ILogger<EmailTemplateController> logger, string TemplateName);

    [LoggerMessage(LogLevel.Error, "Failed to create new template because {Error}")]
    static partial void LogFailedToCreateNewTemplateBecauseError(ILogger<EmailTemplateController> logger, string Error);

    [LoggerMessage(LogLevel.Error, "Failed to add attachment to template with ID: {TemplateID} because {Error}")]
    static partial void LogFailedToAddAttachmentToTemplateWithIdTemplateidBecauseError(ILogger<EmailTemplateController> logger, string TemplateID, string Error);

    [LoggerMessage(LogLevel.Error, "Failed to change template name for template with ID '{TemplateID}' because {Error}")]
    static partial void LogFailedToChangeTemplateNameForTemplateWithIdTemplateidBecauseError(ILogger<EmailTemplateController> logger, string TemplateID, string Error);
}
