using CSharpFunctionalExtensions;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;

public sealed class GetRecordingDownloadUrlQueryHandler(
    IRecordingRegistryPort RegistryPort,
    IRecordingStoragePort StoragePort,
    ILogger<GetRecordingDownloadUrlQueryHandler> Logger)
    : IRequestHandler<GetRecordingDownloadUrlQuery, Result<RecordingDownloadUrlDto>>
{
    public async Task<Result<RecordingDownloadUrlDto>> Handle(
        GetRecordingDownloadUrlQuery request,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Resolving download URL for recording {RecordingId}", request.RecordingId);

        return await RegistryPort
            .GetByIdAsync(request.RecordingId, cancellationToken)
            .Bind(recording => StoragePort.GetDownloadUrlAsync(recording.StorageKey, cancellationToken))
            .Map(url => new RecordingDownloadUrlDto(url))
            .Tap(_ => Logger.LogInformation(
                "Download URL resolved for recording {RecordingId}", request.RecordingId))
            .TapError(error => Logger.LogWarning(
                "Failed to resolve download URL for recording {RecordingId}: {Error}",
                request.RecordingId, error));
    }
}



