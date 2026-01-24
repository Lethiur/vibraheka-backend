using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

/// <summary>
/// Handles the creation of email templates by processing the <see cref="CreateEmailTemplateCommand"/>.
/// </summary>
/// <remarks>
/// This command handler is responsible for creating new email templates by interacting with the appropriate storage
/// and ensuring the current user has the necessary privileges to perform the operation. It uses services such as
/// <see cref="ICurrentUserService"/> to identify the user making the request, <see cref="IEmailTemplateStorageService"/>
/// to manage storage of the email template, and <see cref="IPrivilegeService"/> to enforce authorization rules.
/// </remarks>
/// <param name="currentUser">The service providing details of the currently authenticated user.</param>
/// <param name="storageService">The service responsible for managing email template storage.</param>
/// <param name="privilegeService">The service used for verifying user permissions and roles.</param>
public class CreateEmailTemplateCommandHandler(
    IEmailTemplatesService templateService,
    IEmailTemplateStorageService storageService
) : IRequestHandler<CreateEmailTemplateCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        EmailEntity entity = new EmailEntity() { ID = Guid.NewGuid().ToString() };
        return storageService.SaveTemplate(entity.ID, request.FileStream, cancellationToken)
            .Map(templatePath => entity.Path = templatePath)
            .Bind(_ => templateService.SaveEmailTemplate(entity, cancellationToken))
            .Map(_ => Unit.Value);
    }
}
