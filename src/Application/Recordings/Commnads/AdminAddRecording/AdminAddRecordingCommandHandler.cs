using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Recordings.Entities;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Commnads.AdminAddRecording;

public sealed class AdminAddRecordingCommandHandler(
    IRecordingStoragePort StoragePort,
    IRecordingRegistryPort RegistryPort,
    ICurrentUserService CurrentUserService,
    ILogger<AdminAddRecordingCommandHandler> Logger)
    : IRequestHandler<AdminAddRecordingCommand, Result<AddRecordingResult>>
{
    private const string StoragePrefix = "recordings";

    public async Task<Result<AddRecordingResult>> Handle(
        AdminAddRecordingCommand request,
        CancellationToken cancellationToken)
    {
        string recordingId = Guid.NewGuid().ToString();
        string storageKey = $"{StoragePrefix}/{recordingId}/{request.FileName}";

        Logger.LogInformation(
            "Creating recording entry {RecordingId} with name {Name} and type {Type}",
            recordingId, request.Name, request.Type);

        RecordingEntity entity = new()
        {
            Id = recordingId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            StorageKey = storageKey,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = CurrentUserService.UserId,
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = CurrentUserService.UserId
        };

        Result<string> saveResult = await RegistryPort.SaveRecording(entity, cancellationToken);
        if (saveResult.IsFailure)
        {
            Logger.LogWarning("Failed to persist recording {RecordingId}: {Error}", recordingId, saveResult.Error);
            return Result.Failure<AddRecordingResult>(saveResult.Error);
        }

        Logger.LogInformation(
            "Recording {RecordingId} persisted. Generating pre-signed upload URL for {StorageKey}",
            recordingId, storageKey);

        Result<string> urlResult = await StoragePort.GetUploadUrlAsync(storageKey, cancellationToken);
        if (urlResult.IsFailure)
        {
            Logger.LogWarning("Failed to generate upload URL for recording {RecordingId}: {Error}", recordingId, urlResult.Error);
            return Result.Failure<AddRecordingResult>(urlResult.Error);
        }

        Logger.LogInformation("Recording {RecordingId} ready for direct upload", recordingId);
        return Result.Success(new AddRecordingResult(recordingId, urlResult.Value));
    }
}
