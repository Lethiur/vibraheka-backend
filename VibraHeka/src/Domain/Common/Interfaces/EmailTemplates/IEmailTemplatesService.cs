using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.EmailTemplates;

public interface IEmailTemplatesService
{
    /// <summary>
    /// Retrieves an email template based on the given template ID.
    /// </summary>
    /// <param name="templateID">The identifier of the email template to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{EmailEntity}"/> indicating the success or failure of the operation, along with the retrieved email template if successful.</returns>
    Task<Result<EmailEntity>> GetTemplateByID(string templateID);

    /// <summary>
    /// Retrieves all available email templates.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{IEnumerable{EmailEntity}}"/> indicating the success or failure of the operation and the collection of email templates if successful.</returns>
    Task<Result<IEnumerable<EmailEntity>>> GetAllTemplates(CancellationToken cancellationToken);
}
