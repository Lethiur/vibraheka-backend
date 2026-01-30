using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Queries.GetTemplateContent;

public record GetEmailTemplateContentQuery(string TemplateID) : IRequest<Result<string>>, IRequireAdmin;
