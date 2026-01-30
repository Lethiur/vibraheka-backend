using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;

namespace VibraHeka.Application.EmailTemplates.Commands.EditTemplateName;

/// <summary>
/// Handles the command to edit the name of an email template.
/// </summary>
/// <remarks>
/// This handler processes the <see cref="EditTemplateNameCommand"/> request to update the name of an existing email template.
/// The command requires administrative privileges to execute, as defined by the implementation of <see cref="IRequireAdmin"/>.
/// </remarks>
/// <param name="emailTemplatesService">
/// The service used to perform email template-related operations, including updating the template name.
/// </param>
/// <seealso cref="EditTemplateNameCommand"/>
/// <seealso cref="IEmailTemplatesService"/>
public class EditTemplateNameCommandHandler(IEmailTemplatesService emailTemplatesService) : IRequestHandler<EditTemplateNameCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(EditTemplateNameCommand request, CancellationToken cancellationToken)
    {
        return emailTemplatesService.EditTemplateName(request.TemplateID, request.NewTemplateName, cancellationToken);
    }
}
