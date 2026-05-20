using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.Orders;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Queries.GetRecordingDownloadUrl;

public sealed class GetRecordingDownloadUrlQueryHandler(
    IRecordingRegistryPort RegistryPort,
    ICurrentUserService CurrentUserService,
    ISubscriptionService SubscriptionService,
    IRecordingStoragePort StoragePort,
    ILogger<GetRecordingDownloadUrlQueryHandler> Logger)
    : IRequestHandler<GetRecordingDownloadUrlQuery, Result<RecordingDownloadUrlDto>>
{
    public async Task<Result<RecordingDownloadUrlDto>> Handle(
        GetRecordingDownloadUrlQuery request,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Resolving download URL for recording {RecordingId}", request.RecordingId);

        Result<RecordingEntity> recordingRecordResult = await RegistryPort.GetByIdAsync(request.RecordingId, cancellationToken);

        if (recordingRecordResult.IsSuccess && recordingRecordResult.Value.IsForSubscribers())
        {
            Result<SubscriptionEntity> subscriptionForUser = await SubscriptionService.GetSubscriptionForUser(CurrentUserService.UserId!, cancellationToken);

            if (subscriptionForUser.IsFailure)
            {
                Logger.LogWarning("Failed to resolve download URL for recording {RecordingId}: {Error}", request.RecordingId, subscriptionForUser.Error);
                return Result.Failure<RecordingDownloadUrlDto>(subscriptionForUser.Error);
            }

            if (!subscriptionForUser.Value.IsActive())
            {
                Logger.LogWarning("Failed to resolve download URL for recording {RecordingId}: {Error}", request.RecordingId, RecordingErrors.OnlyForSubscribers);
                return Result.Failure<RecordingDownloadUrlDto>(RecordingErrors.OnlyForSubscribers);
            }
        }

        return await recordingRecordResult
            .Bind(recording => StoragePort.GetDownloadUrlAsync(recording.Id, cancellationToken))
            .Map(url => new RecordingDownloadUrlDto(url))
            .Tap(_ => Logger.LogInformation(
                "Download URL resolved for recording {RecordingId}", request.RecordingId))
            .TapError(error => Logger.LogWarning(
                "Failed to resolve download URL for recording {RecordingId}: {Error}",
                request.RecordingId, error));
    }
}



