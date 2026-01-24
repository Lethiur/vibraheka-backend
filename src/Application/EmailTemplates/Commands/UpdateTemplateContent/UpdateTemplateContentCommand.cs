using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Commands.UpdateTemplate;

public record UpdateTemplateCommand(string TemplateID, Stream TemplateStream) : IRequest<Result<Unit>>,  IRequireAdmin;
