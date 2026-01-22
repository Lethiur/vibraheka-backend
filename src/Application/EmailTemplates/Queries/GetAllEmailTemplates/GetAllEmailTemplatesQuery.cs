using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;

public record GetAllEmailTemplatesQuery() : IRequest<Result<IEnumerable<EmailEntity>>>;
