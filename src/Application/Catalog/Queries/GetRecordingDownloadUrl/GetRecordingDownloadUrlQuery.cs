using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;

public sealed record GetRecordingDownloadUrlQuery(string RecordingId)
    : IRequest<Result<RecordingDownloadUrlDto>>;

