using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.EmailTemplates.Queries.GetAllEmailTemplates;

/// <summary>
/// Represents a query to retrieve all email templates.
/// </summary>
/// <remarks>
/// This query is intended to fetch a collection of email templates from the system.
/// It enforces the requirement that the querying user has administrative privileges
/// by implementing the <see cref="IRequireAdmin"/> interface.
/// </remarks>
public record GetAllEmailTemplatesQuery() : IRequest<Result<IEnumerable<EmailEntity>>>, IRequireAdmin;
