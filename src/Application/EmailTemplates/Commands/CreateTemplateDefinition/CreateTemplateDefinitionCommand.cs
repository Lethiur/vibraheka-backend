using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Commands.CreateTemplateDefinition;

public record CreateTemplateDefinitionCommand(string TempateName) : IRequest<Result<string>>, IRequireAdmin;
