using CSharpFunctionalExtensions;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

public record CreateEmailTemplateCommand(Stream FileStream, string TemplateName) : IRequest<Result<Unit>>
{
}
