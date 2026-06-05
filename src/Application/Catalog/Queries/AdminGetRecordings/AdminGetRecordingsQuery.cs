using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Queries.AdminGetRecordings;

public record AdminGetRecordingsQuery() : IRequireAdmin, IRequest<Result<IEnumerable<RecordingEntity>>>;
