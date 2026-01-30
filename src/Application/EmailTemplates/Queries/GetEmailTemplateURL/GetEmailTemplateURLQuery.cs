using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;

namespace VibraHeka.Application.EmailTemplates.Queries.GetEmailTemplateURL;

public record GetEmailTemplateURLQuery(string TemplateID) : IRequest<Result<string>>, IRequireAdmin;
