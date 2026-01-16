using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.EmailTemplates;

public interface IEmailTemplatesRepository
{
    /// <summary>
    /// Retrieves an email template by its unique identifier.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result"/>
    /// object wrapping the <see cref="EmailEntity"/> if found, or an error if the operation fails.</returns>
    Task<Result<EmailEntity>> GetTemplateByID(string templateID);

    /// <summary>
    /// Saves an email template to the repository.
    /// </summary>
    /// <param name="template">The email template to be saved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result"/>
    /// wrapping a <see cref="Unit"/> indicating success, or an error if the operation fails.</returns>
    Task<Result<Unit>> SaveTemplate(EmailEntity template);
}
