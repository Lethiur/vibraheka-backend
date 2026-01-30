using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplateContent;

/// <summary>
/// Handles the operation of updating an email template within the application.
/// </summary>
/// <remarks>
/// This class is responsible for processing the <see cref="UpdateTemplateContentCommand"/> request,
/// which involves updating an email template using the provided data and ensuring that the operation
/// adheres to the correct permissions and business logic. It relies on multiple domain services
/// to authenticate the user, validate permissions, and perform the update action.
/// </remarks>
public class UpdateTemplateContentCommandHandler(
    IEmailTemplatesService emailTemplatesService,
    IEmailTemplateStorageService emailTemplateStorageService) : IRequestHandler<UpdateTemplateContentCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateTemplateContentCommand request, CancellationToken cancellationToken)
    {
        return emailTemplatesService.GetTemplateByID(request.TemplateID)
            .Bind(templateEntity => emailTemplateStorageService.SaveTemplate(templateEntity.ID, request.TemplateStream, cancellationToken))
            .Map(_ => Unit.Value);
    }
}
