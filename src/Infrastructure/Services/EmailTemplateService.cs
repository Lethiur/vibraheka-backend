using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

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
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{EmailEntity}"/> object
    /// wrapping the email template if found, or an error if the operation fails.</returns>
    public Task<Result<EmailEntity>> GetTemplateByID(Guid templateID, CancellationToken cancellationToken)
    {
        return Maybe.From(templateID)
            .Where(tid => tid != Guid.Empty)
            .ToResult(EmailTemplateErrors.InvalidTempalteID)
            .BindTry(async id => await EmailTemplateRepository.GetTemplateByID(id, cancellationToken))
            .MapError(error =>
            {
                return error switch
                {
                    GenericPersistenceErrors.NoRecordsFound => EmailTemplateErrors.TemplateNotFound,
                    _ => error
                };
            });
    }

    /// <summary>
    /// Retrieves all email templates from the system.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result"/>
    /// wrapping a collection of <see cref="EmailEntity"/> objects or an error if the operation fails.</returns>
    public Task<Result<IEnumerable<EmailEntity>>> GetAllTemplates(CancellationToken cancellationToken)
    {
        return EmailTemplateRepository.GetAllTemplates(cancellationToken);
    }

    /// <summary>
    /// Saves a new or updated email template to the repository.
    /// </summary>
    /// <param name="emailTemplate">The email template entity to save.</param>
    /// <param name="cancellationToken">The token to preemptively cancel the task if needed</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{String}"/> object
    /// wrapping the identifier of the saved email template if successful, or an error if the operation fails.</returns>
    public async Task<Result<string>> SaveEmailTemplate(EmailEntity emailTemplate, CancellationToken cancellationToken)
    {
        return await Maybe.From(emailTemplate)
            .ToResult(EmailTemplateErrors.InvalidTemplateEntity)
            .BindTry(tpl => EmailTemplateRepository.SaveTemplate(tpl, cancellationToken)
                .Map(_ => emailTemplate.ID));
    }

    /// <summary>
    /// Updates the name of an existing email template.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to be updated.</param>
    /// <param name="newTemplateName">The new name to assign to the email template.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{Unit}"/> indicating success or failure of the operation.</returns>
    public Task<Result<Unit>> EditTemplateName(Guid templateID, string newTemplateName, CancellationToken cancellationToken)
    {
        return
            Maybe.From(templateID)
                .ToResult(EmailTemplateErrors.InvalidTempalteID)
                .BindTry(template => EmailTemplateRepository.GetTemplateByID(template, cancellationToken))
                .Ensure(tpl => tpl != null, EmailTemplateErrors.TemplateNotFound)
                .TapTry(entity =>
                {
                    entity.Name = newTemplateName;
                    entity.LastModified = DateTime.UtcNow;
                })
                .BindTry(entity => EmailTemplateRepository.SaveTemplate(entity, cancellationToken))
                .Map(_ => Unit.Value);
    }
}
