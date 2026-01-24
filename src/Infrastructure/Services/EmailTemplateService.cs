using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Service for managing and retrieving email templates within the system.
/// </summary>
public class EmailTemplateService(IEmailTemplatesRepository EmailTemplateRepository) : IEmailTemplatesService
{
    /// <summary>
    /// Retrieves an email template by its unique identifier.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{EmailEntity}"/> object
    /// wrapping the email template if found, or an error if the operation fails.</returns>
    public async Task<Result<EmailEntity>> GetTemplateByID(string templateID)
    {
        return await Maybe.From(templateID)
            .Where(tid => !string.IsNullOrWhiteSpace(tid))
            .ToResult(EmailTemplateErrors.InvalidTempalteID)
            .Bind(async (id) => await EmailTemplateRepository.GetTemplateByID(id))
            .Ensure(tpl => tpl != null, EmailTemplateErrors.TemplateNotFound);
    }

    /// <summary>
    /// Retrieves all email templates from the system.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result"/>
    /// wrapping a collection of <see cref="EmailEntity"/> objects or an error if the operation fails.</returns>
    public Task<Result<IEnumerable<EmailEntity>>> GetAllTemplates(CancellationToken cancellationToken )
    {
       return EmailTemplateRepository.GetAllTemplates(cancellationToken);
    }

    /// <summary>
    /// Saves a new or updated email template to the repository.
    /// </summary>
    /// <param name="emailTemplate">The email template entity to save.</param>
    /// <param name="token">The token to preemptively cancel the task if needed</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{String}"/> object
    /// wrapping the identifier of the saved email template if successful, or an error if the operation fails.</returns>
    public async Task<Result<string>> SaveEmailTemplate(EmailEntity emailTemplate, CancellationToken token)
    {
        return await Maybe.From(emailTemplate)
            .Where(tpl => tpl != null)
            .ToResult(EmailTemplateErrors.InvalidTemplateEntity)
            .MapTry(async tpl =>
            {
                 await EmailTemplateRepository.SaveTemplate(tpl, token);
                 return tpl.ID;
            });
    }

    /// <summary>
    /// Updates the name of an existing email template.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to be updated.</param>
    /// <param name="newTemplateName">The new name to assign to the email template.</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating success or failure of the operation.</returns>
    public Task<Result<Unit>> EditTemplateName(string templateID, string newTemplateName, CancellationToken token)
    {
        return GetTemplateByID(templateID)
            .Tap(entity =>
            {
                entity.Name = newTemplateName;
                entity.LastModified = DateTime.UtcNow;
                EmailTemplateRepository.SaveTemplate(entity, token);
            }).Map(_ => Unit.Value);
    }
}
