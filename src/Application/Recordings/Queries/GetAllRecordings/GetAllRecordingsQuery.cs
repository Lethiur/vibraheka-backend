using CSharpFunctionalExtensions;
namespace VibraHeka.Application.Recordings.Queries.GetAllRecordings;

public record GetAllRecordingsQuery : IRequest<Result<IEnumerable<RecordingDto>>>;
