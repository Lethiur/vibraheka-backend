using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Settings.Queries.GetTemplateForAction;

/// <summary>
/// Represents a query to retrieve email templates associated with all actions
/// </summary>
/// <remarks>
/// This query encapsulates the information required to fetch a collection of <see cref="TemplateForActionEntity"/> objects.
/// It is typically handled by an implementation of <see cref="IRequestHandler{TRequest, TResponse}"/>.
/// </remarks>
public record class GetTemplatesForActionQuery : IRequest<Result<IEnumerable<TemplateForActionEntity>>>;
