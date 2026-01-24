using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

public record CreateEmailTemplateCommand(Stream FileStream, string TemplateName) : IRequest<Result<Unit>>, IRequireAdmin
{
}
