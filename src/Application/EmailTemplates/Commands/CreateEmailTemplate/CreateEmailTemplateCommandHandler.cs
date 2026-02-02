using CSharpFunctionalExtensions;
using VibraHeka.Application.EmailTemplates.Commands.CreateEmail;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmailTemplate;

/// <summary>
/// Handles the creation of email templates by processing the given command and interacting with
/// the necessary services for storage and template management.
/// </summary>
/// <remarks>
/// This handler processes the <see cref="CreateEmailTemplateCommand"/>, which contains the file stream
/// and template name. It utilizes the <see cref="IEmailTemplateStorageService"/> to save the file
/// and the <see cref="IEmailTemplatesService"/> to persist the template's metadata.
/// </remarks>
/// <param name="templateService">Service for managing email template metadata.</param>
/// <param name="storageService">Service for handling email template storage in external systems.</param>
public class CreateEmailTemplateCommandHandler(
    IEmailTemplatesService templateService,
    IEmailTemplateStorageService storageService,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateEmailTemplateCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        EmailEntity entity = new EmailEntity()
        {
            ID = Guid.NewGuid().ToString(),
            Name = request.TemplateName,
            Created = DateTime.UtcNow,
            CreatedBy = currentUserService.UserId,
            LastModified = DateTime.UtcNow,
            LastModifiedBy = currentUserService.UserId
        };
        return storageService.SaveTemplate(entity.ID, request.FileStream, cancellationToken)
            .Map(templatePath => entity.Path = templatePath)
            .Bind(_ => templateService.SaveEmailTemplate(entity, cancellationToken))
            .Map(_ => Unit.Value);
    }
}
