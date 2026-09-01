using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
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
    ILogger<EmailTemplateController> logger,
    EmailTemplateMapper mapper) : IEmailTemplatesController
{
    /// <summary>
    /// Retrieves all email templates.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a list of email templates if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    public override async Task<ActionResult<GetTemplatesResponse>> GetAllEmailTemplates(CancellationToken cancellationToken = default)
    {
        Result<IEnumerable<EmailEntity>> result = await mediator.Send(new GetAllEmailTemplatesQuery(), cancellationToken);

        if (result.IsFailure)
        {
            LogFailedToGetAllTemplatesBecauseError(logger, result.Error);
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
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="ActionResult{CreateEmailTemplateResponse}"/> containing the ID of the newly created email template
    /// if the operation is successful, or an error response in case of failure.
    /// </returns>
    public override async Task<ActionResult<CreateEmailTemplateResponse>> CreateEmailTemplate(
        CreateEmailTemplateRequest body, CancellationToken cancellationToken =default)
    {
        CreateTemplateDefinitionCommand command = new(body.Name);
        Result<string> result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            LogFailedToCreateTheTemplateSkeletonBecauseError(logger, result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        logger.LogInformation(
            "Successfully created template skeleton for template {TemplateName} with ID: {TemplateID}", body.Name,
            result.Value);
        return new OkObjectResult(new CreateEmailTemplateResponse { TemplateId = mapper.ToGuid(result.Value) });
    }

    /// <summary>
    /// Adds a new attachment to an existing email template.
    /// </summary>
    /// <param name="attachmentName">The name of the attachment to be added to the email template.</param>
    /// <param name="templateID">The unique identifier of the email template to which the attachment will be added.</param>
    /// <param name="file">The file to be added as an attachment to the email template.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating success with an empty response body or an appropriate error response.
    /// </returns>
    public override async Task<ActionResult<AddAttachmentResponse>> AddAttachmentToEmailTemplate(string attachmentName,
        Guid? templateID, IFormFile file, CancellationToken cancellationToken = default)
    {
        AddAttachmentCommand command = new(file.OpenReadStream(), templateID ?? Guid.Empty, attachmentName);
        Result<string> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            LogFailedToAddAttachmentToTemplateWithIdTemplateIdBecauseError(logger, templateID?.ToString() ?? "fucked up", result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new AddAttachmentResponse { Url = mapper.ToUri(result.Value) });
    }


    /// <summary>
    /// Updates the name of an existing email template.
    /// </summary>
    /// <param name="body">
    /// Contains the template ID and the new name to be assigned to the template.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating whether the operation was successful. Returns an error response if the operation fails,
    /// including an unauthorized result if the caller lacks permissions.
    /// </returns>
    public override async Task<IActionResult> ChangeTemplateName(UpdateEmailTemplateNameRequest body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Changing template name for template with ID '{TemplateID}' to '{NewTemplateName}'",
            body.TemplateID, body.Name);
        EditTemplateNameCommand command = new(body.TemplateID, body.Name);
        Result<Unit> result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            logger.LogError("Failed to change template name for template with ID '{TemplateID}' because {Error}",
                body.TemplateID, result.Error);
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
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that indicates the result of the operation.
    /// Returns a <see cref="NoContentResult"/> if the update is successful, or a <see cref="BadRequestObjectResult"/> if the operation fails.
    /// </returns>
    
    public override async Task<IActionResult> UpdateEmailTemplateContent(Guid? templateID, IFormFile templateFile, CancellationToken cancellationToken = default)
    {
        UpdateTemplateContentCommand command = new(templateID ?? Guid.Empty, templateFile.OpenReadStream());
        Result<Unit> result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            LogFailedToChangeTemplateNameForTemplateWithIdTemplateIdBecauseError(logger, templateID?.ToString() ?? "null",
                result.Error);
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }
        return new NoContentResult();
    }


    /// <summary>
    /// Retrieves the download URL for a specified email template.
    /// </summary>
    /// <param name="templateId">The unique identifier of the email template.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the download URL if successful,
    /// or an appropriate error response if the operation fails.
    /// </returns>
    public override async Task<ActionResult<GetEmailTemplateContentResponse>> GetEmailTemplateContent(string templateId, CancellationToken cancellationToken = default)
    {
        GetEmailTemplateURLQuery query = new(templateId);
        Result<string> result = await mediator.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return new BadRequestObjectResult(new BadRequestResponse { ErrorCode = result.Error });
        }

        return new OkObjectResult(new GetEmailTemplateContentResponse { Url =  mapper.ToUri(result.Value) });
    }



    [LoggerMessage(LogLevel.Error, "Failed to get all templates because {error}")]
    static partial void LogFailedToGetAllTemplatesBecauseError(ILogger<EmailTemplateController> logger, string error);

    [LoggerMessage(LogLevel.Information, "Successfully created new template {templateName}")]
    static partial void LogSuccessfullyCreatedNewTemplateTemplateName(ILogger<EmailTemplateController> logger,
        string templateName);

    [LoggerMessage(LogLevel.Error, "Failed to create new template because {error}")]
    static partial void LogFailedToCreateNewTemplateBecauseError(ILogger<EmailTemplateController> logger, string error);

    [LoggerMessage(LogLevel.Error, "Failed to add attachment to template with ID: {templateID} because {error}")]
    static partial void LogFailedToAddAttachmentToTemplateWithIdTemplateIdBecauseError(
        ILogger<EmailTemplateController> logger, string templateID, string error);

    [LoggerMessage(LogLevel.Error,
        "Failed to change template name for template with ID '{templateID}' because {error}")]
    static partial void LogFailedToChangeTemplateNameForTemplateWithIdTemplateIdBecauseError(
        ILogger<EmailTemplateController> logger, string templateID, string error);

    [LoggerMessage(LogLevel.Error, "Failed to create the template skeleton because: {error}")]
    static partial void LogFailedToCreateTheTemplateSkeletonBecauseError(ILogger<EmailTemplateController> logger,
        string error);
    
}
