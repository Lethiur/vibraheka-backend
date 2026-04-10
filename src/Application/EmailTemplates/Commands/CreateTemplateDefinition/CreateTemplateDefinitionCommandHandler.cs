using CSharpFunctionalExtensions;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Services;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;

public class CreateTemplateDefinitionCommandHandler(
    ICurrentUserService currentUserService,
    EmailTemplatePort templatesService) : IRequestHandler<CreateTemplateDefinitionCommand, Result<string>>
{
    public Task<Result<string>> Handle(CreateTemplateDefinitionCommand request, CancellationToken cancellationToken)
    {
        return templatesService.SaveEmailTemplate(
            new EmailTemplateEntity
            {
                TemplateID = Guid.NewGuid().ToString(),
                Name = request.TempateName,
                CreatedBy = currentUserService.UserId,
                Created = DateTime.UtcNow
            }, cancellationToken);
    }
}
