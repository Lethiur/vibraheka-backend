using CSharpFunctionalExtensions;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Application.Catalog.Queries.GetAllRecordings;

public record GetAllRecordingsQuery : IRequest<Result<IEnumerable<RecordingEntity>>>;
