using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
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
            .Bind(async (id) => await EmailTemplateRepository.GetTemplateByID(id));
    }
}
