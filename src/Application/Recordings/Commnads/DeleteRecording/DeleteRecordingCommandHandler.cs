using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Commnads.DeleteRecording;

public class DeleteRecordingCommandHandler(
    IRecordingRegistryPort RegistryPort,
    IRecordingStoragePort StoragePort,
    ILogger<DeleteRecordingCommandHandler> Logger)
    : IRequestHandler<DeleteRecordingCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteRecordingCommand request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Deleting recording {RecordingId}", request.RecordingId);

        Result<RecordingEntity> entityResult = await RegistryPort.GetByIdAsync(request.RecordingId, cancellationToken);

        if (entityResult.IsFailure)
        {
            Logger.LogWarning(
                "Recording {RecordingId} not found: {Error}",
                request.RecordingId,
                entityResult.Error);
            return Result.Failure<Unit>(entityResult.Error);
        }

        RecordingEntity entity = entityResult.Value;

        Result deleteFileResult = await StoragePort.DeleteFileAsync(entity.Id, cancellationToken);

        if (deleteFileResult.IsFailure)
        {
            Logger.LogWarning(
                "Failed to delete file for recording {RecordingId}: {Error}",
                request.RecordingId,
                deleteFileResult.Error);
            return Result.Failure<Unit>(deleteFileResult.Error);
        }

        Result deleteRecordResult = await RegistryPort.DeleteRecordingAsync(entity, cancellationToken);

        if (deleteRecordResult.IsFailure)
        {
            Logger.LogWarning(
                "Failed to delete recording metadata {RecordingId}: {Error}",
                request.RecordingId,
                deleteRecordResult.Error);
            return Result.Failure<Unit>(deleteRecordResult.Error);
        }

        Logger.LogInformation("Recording {RecordingId} successfully deleted", request.RecordingId);
        return Unit.Value;
    }
}

