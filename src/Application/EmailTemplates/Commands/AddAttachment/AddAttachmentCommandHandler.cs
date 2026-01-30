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
/// <param name="templatesService">
/// A service responsible for handling operations on email templates.
/// </param>
/// <param name="emailTemplateStorageService">
/// A service for managing storage of email template attachments
/// in the underlying data store.
/// </param>
public class AddAttachmentCommandHandler(
    IEmailTemplatesService templatesService,
    IEmailTemplateStorageService emailTemplateStorageService
) : IRequestHandler<AddAttachmentCommand, Result<string>>
{
    public Task<Result<string>> Handle(AddAttachmentCommand request, CancellationToken cancellationToken)
    {
        return templatesService.GetTemplateByID(request.TemplateId)
            .Bind(templateEntity => emailTemplateStorageService.AddAttachment(templateEntity.ID, request.FileStream, request.AttachmentName, cancellationToken)
                .Tap(url => templateEntity.Attachments.Add(url))
                .Map(_ => templateEntity)
                .Bind(entity => templatesService.SaveEmailTemplate(templateEntity, cancellationToken))
                .Map(entity => templateEntity.Attachments.Last()));
    }
}
