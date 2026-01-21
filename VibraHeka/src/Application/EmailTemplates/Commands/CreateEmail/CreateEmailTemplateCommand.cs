using CSharpFunctionalExtensions;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateEmail;

public record CreateEmailTemplateCommand(Stream fileStream, string templateName) : IRequest<Result<Unit>>;
