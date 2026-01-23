using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.AddAttachment;

/// <summary>
/// Handles the addition of attachments to email templates within the system.
/// </summary>
/// <remarks>
/// The class implements the <see cref="IRequestHandler{TRequest, TResponse}"/> interface,
/// providing logic to process <see cref="AddAttachmentCommand"/> requests.
/// </remarks>
/// <param name="privilegeService">
/// A service for verifying user privileges to ensure the user has permission
/// to modify the email template or add attachments.
/// </param>
/// <param name="currentUserService">
/// A service providing information about the current user, such as their UserId.
/// </param>
/// <param name="templatesService">
/// A service responsible for handling operations on email templates.
/// </param>
/// <param name="emailTemplateStorageService">
/// A service for managing storage of email template attachments
/// in the underlying data store.
/// </param>
public class AddAttachmentCommandHandler(
    IPrivilegeService privilegeService,
    ICurrentUserService currentUserService,
    IEmailTemplatesService templatesService,
    IEmailTemplateStorageService emailTemplateStorageService
    ) : IRequestHandler<AddAttachmentCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(AddAttachmentCommand request, CancellationToken cancellationToken)
    {
        return Maybe.From(currentUserService.UserId)
            .Where(userID => !string.IsNullOrEmpty(userID) && !string.IsNullOrWhiteSpace(userID))
            .ToResult(UserErrors.InvalidUserID)
            .Bind(async userID => await privilegeService.HasRoleAsync(userID, UserRole.Admin))
            .Ensure(hasRole => hasRole, UserErrors.NotAuthorized)
            .Bind(_ => templatesService.GetTemplateByID(request.TemplateId))
            .Ensure(template => template != null, EmailTemplateErrors.TemplateNotFound)
            .Bind(templateEntity =>
                emailTemplateStorageService
                    .AddAttachment(templateEntity.ID, request.FileStream, request.AttachmentName, cancellationToken)
                    .Tap(url => templateEntity.Attachments.Add(url))
                    .Map(_ => templateEntity))
            .Bind(templateEntity => templatesService.SaveEmailTemplate(templateEntity, cancellationToken))
            .Map(_ => Unit.Value);
    }
}
