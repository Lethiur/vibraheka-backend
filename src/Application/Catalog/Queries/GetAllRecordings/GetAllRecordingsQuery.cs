using CSharpFunctionalExtensions;
using VibraHeka.Application.Catalog.Models;

namespace VibraHeka.Application.Catalog.Queries.GetAllRecordings;

public record GetAllRecordingsQuery : IRequest<Result<IEnumerable<RecordingDto>>>;
