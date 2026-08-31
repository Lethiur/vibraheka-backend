using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Catalog.Queries.GetRecordingDownloadUrl;

public sealed record GetRecordingDownloadUrlQuery(string RecordingId)
    : IRequest<Result<string>>;

