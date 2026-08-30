using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.EmailTemplates.Commands.AddAttachment;
using VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;
using VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;
using VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;
using VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;
using VibraHeka.Domain.Entities;
using VibraHeka.Web.EmailTemplates;

namespace VibraHeka.Web.Controllers.EmailTemplates;

/// <summary>
/// Controller responsible for handling operations related to email templates.
/// </summary>
public partial class EmailTemplateController(
    IMediator mediator,
    ILogger<EmailTemplateController> Logger,
    EmailTemplateMapper mapper) : IEmailTemplatesController
{
    /// <summary>
    /// Retrieves all email templates.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of email templates if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    public override async Task<ActionResult<GetTemplatesResponse>> GetAllEmailTemplates()
    {
        Result<IEnumerable<EmailEntity>> result = await mediator.Send(new GetAllEmailTemplatesQuery());

        if (result.IsFailure)
        {
            LogFailedToGetAllTemplatesBecauseError(Logger, result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new GetTemplatesResponse { Templates = [.. result.Value.Select(mapper.ToDTO)] });
    }

    /// <summary>
    /// Creates a new email template.
    /// </summary>
    /// <param name="body">
    /// An object of type <see cref="CreateEmailTemplateRequest"/> containing the name of the template to be created.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{CreateEmailTemplateResponse}"/> containing the ID of the newly created email template
    /// if the operation is successful, or an error response in case of failure.
    /// </returns>
    public override async Task<ActionResult<CreateEmailTemplateResponse>> CreateEmailTemplate(
        CreateEmailTemplateRequest body)
    {
        CreateTemplateDefinitionCommand command = new(body.Name);
        Result<string> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            LogFailedToCreateTheTeamplateSkeletonBecauseError(Logger, result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        Logger.LogInformation(
            "Successfully created template skeleton for template {TemplateName} with ID: {TemplateID}", body.Name,
            result.Value);
        return new OkObjectResult(new CreateEmailTemplateResponse { TemplateId = mapper.ToGuid(result.Value) });
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
    public override async Task<ActionResult<AddAttachmentResponse>> AddAttachmentToEmailTemplate(string attachmentName,
        Guid? templateID, IFormFile file)
    {
        AddAttachmentCommand command = new(file.OpenReadStream(), templateID?.ToString() ?? "", attachmentName);
        Result<string> result = await mediator.Send(command);

        if (result.IsFailure)
        {
            LogFailedToAddAttachmentToTemplateWithIdTemplateidBecauseError(Logger, templateID?.ToString() ?? "fucked up", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new AddAttachmentResponse { Url = mapper.ToUri(result.Value) });
    }


    /// <summary>
    /// Updates the name of an existing email template.
    /// </summary>
    /// <param name="body">
    /// An <see cref="EditTemplateNameRequest"/> containing the template ID and the new name to be assigned to the template.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating whether the operation was successful. Returns an error response if the operation fails,
    /// including an unauthorized result if the caller lacks permissions.
    /// </returns>
    public override async Task<IActionResult> ChangeTemplateName(UpdateEmailTemplateNameRequest body)
    {
        Logger.LogInformation("Changing template name for template with ID '{TemplateID}' to '{NewTemplateName}'",
            body.TemplateID, body.Name);
        EditTemplateNameCommand command = new(body.TemplateID.ToString(), body.Name);
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            Logger.LogError("Failed to change template name for template with ID '{TemplateID}' because {Error}",
                body.TemplateID, result.Error);
            if (result.Error == UserErrors.NotAuthorized)
            {
                return new UnauthorizedResult();
            }

            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new NoContentResult();
    }


    /// <summary>
    /// Updates the content of an email template.
    /// </summary>
    /// <param name="templateID">
    /// The unique identifier of the email template to update.
    /// </param>
    /// <param name="templateFile">
    /// The file containing the new content for the email template.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> that indicates the result of the operation.
    /// Returns a <see cref="NoContentResult"/> if the update is successful, or a <see cref="BadRequestObjectResult"/> if the operation fails.
    /// </returns>
    public override async Task<IActionResult> UpdateEmailTemplateContent(Guid? templateID, IFormFile templateFile)
    {
        UpdateTemplateContentCommand command = new(templateID.ToString(), templateFile.OpenReadStream());
        Result<Unit> result = await mediator.Send(command);
        if (result.IsFailure)
        {
            LogFailedToChangeTemplateNameForTemplateWithIdTemplateidBecauseError(Logger, templateID?.ToString() ?? "null",
                result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }
        return new NoContentResult();
    }


    /// <summary>
    /// Retrieves the download URL for a specified email template.
    /// </summary>
    /// <param name="templateId">The unique identifier of the email template.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the download URL if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    public override async Task<ActionResult<GetEmailTemplateContentResponse>> GetEmailTemplateContent(string templateId)
    {
        GetEmailTemplateURLQuery query = new(templateId);
        Result<string> result = await mediator.Send(query);
        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new GetEmailTemplateContentResponse { Url =  mapper.ToUri(result.Value) });
    }



    [LoggerMessage(LogLevel.Error, "Failed to get all templates because {Error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<EmailTemplateController> logger, string Error);

    [LoggerMessage(LogLevel.Information, "Successfully created new template {TemplateName}")]
    static partial void LogSuccessfullyCreatedNewTemplateTemplatename(ILogger<EmailTemplateController> logger,
        string TemplateName);

    [LoggerMessage(LogLevel.Error, "Failed to create new template because {Error}")]
    static partial void LogFailedToCreateNewTemplateBecauseError(ILogger<EmailTemplateController> logger, string Error);

    [LoggerMessage(LogLevel.Error, "Failed to add attachment to template with ID: {TemplateID} because {Error}")]
    static partial void LogFailedToAddAttachmentToTemplateWithIdTemplateidBecauseError(
        ILogger<EmailTemplateController> logger, string TemplateID, string Error);

    [LoggerMessage(LogLevel.Error,
        "Failed to change template name for template with ID '{TemplateID}' because {Error}")]
    static partial void LogFailedToChangeTemplateNameForTemplateWithIdTemplateidBecauseError(
        ILogger<EmailTemplateController> logger, string TemplateID, string Error);

    [LoggerMessage(LogLevel.Error, "Failed to create the teamplate skeleton because: {Error}")]
    static partial void LogFailedToCreateTheTeamplateSkeletonBecauseError(ILogger<EmailTemplateController> logger,
        string Error);
    
}
