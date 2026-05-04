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
    public async Task<Result<AddRecordingResult>> Handle(
        AdminAddRecordingCommand request,
        CancellationToken cancellationToken)
    {
        string recordingId = Guid.NewGuid().ToString();

        Logger.LogInformation(
            "Creating recording entry {RecordingId} with name {Name}, tier {Tier} and type {Type}",
            recordingId, request.Name, request.Tier, request.Type);

        RecordingEntity entity = new()
        {
            Id = recordingId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Tier = request.Tier,
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
            "Recording {RecordingId} persisted. Generating pre-signed upload URL for",
            recordingId);

        Result<string> urlResult = await StoragePort.GetUploadUrlAsync(entity.Id, cancellationToken);
        if (urlResult.IsFailure)
        {
            Logger.LogWarning("Failed to generate upload URL for recording {RecordingId}: {Error}", recordingId, urlResult.Error);
            return Result.Failure<AddRecordingResult>(urlResult.Error);
        }

        Logger.LogInformation("Recording {RecordingId} ready for direct upload", entity.Id);
        return Result.Success(new AddRecordingResult(entity.Id, urlResult.Value));
    }
}
