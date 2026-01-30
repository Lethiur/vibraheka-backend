using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;

public class CreateTemplateDefinitionCommandHandler(
    ICurrentUserService currentUserService,
    IEmailTemplatesService templatesService) : IRequestHandler<CreateTemplateDefinitionCommand, Result<string>>
{
    public Task<Result<string>> Handle(CreateTemplateDefinitionCommand request, CancellationToken cancellationToken)
    {
        return templatesService.SaveEmailTemplate(
            new EmailEntity
            {
                ID = Guid.NewGuid().ToString(),
                Name = request.TempateName,
                CreatedBy = currentUserService.UserId,
                Created = DateTime.UtcNow
            }, cancellationToken);
    }
}
