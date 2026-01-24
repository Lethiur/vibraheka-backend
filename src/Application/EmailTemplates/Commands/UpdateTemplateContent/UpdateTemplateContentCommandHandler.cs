using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplate;

/// <summary>
/// Handles the operation of updating an email template within the application.
/// </summary>
/// <remarks>
/// This class is responsible for processing the <see cref="UpdateTemplateCommand"/> request,
/// which involves updating an email template using the provided data and ensuring that the operation
/// adheres to the correct permissions and business logic. It relies on multiple domain services
/// to authenticate the user, validate permissions, and perform the update action.
/// </remarks>
public class UpdateTemplateCommandHandler(
    IEmailTemplatesService emailTemplatesService,
    IEmailTemplateStorageService emailTemplateStorageService) : IRequestHandler<UpdateTemplateCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        return emailTemplatesService.GetTemplateByID(request.TemplateID)
            .Bind(templateEntity => emailTemplateStorageService.SaveTemplate(templateEntity.ID, request.TemplateStream, cancellationToken))
            .Map(_ => Unit.Value);
    }
}
