using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Commnads.AdminAddRecording;

public class AdminAddRecordingCommandHandler(
    IRecordingStoragePort StoragePort,
    IRecordingRegistryPort RegistryPort,
    ICurrentUserService CurrentUserService,
    ILogger<AdminAddRecordingCommandHandler> Logger)
    : IRequestHandler<AdminAddRecordingCommand, Result<string>>
{
    public Task<Result<string>> Handle(AdminAddRecordingCommand request, CancellationToken cancellationToken)
    {
        string recordingId = Guid.NewGuid().ToString();

        Logger.LogInformation(
            "Uploading recording {RecordingId} with name {Name} and type {Type}",
            recordingId, request.Name, request.Type);

        return StoragePort
            .UploadAsync(recordingId, request.FileStream, request.FileName, cancellationToken)
            .Tap(storageKey =>  Logger.LogInformation(
                "Saving recording {RecordingId} with storage key {StorageKey}",
                recordingId, storageKey))
            .BindTry(storageKey =>
            {
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

                return RegistryPort.SaveAsync(entity, cancellationToken);
            })
            .Tap(id => Logger.LogInformation(
                "Recording {RecordingId} successfully added", id))
            .TapError(error => Logger.LogWarning(
                "Failed to add recording {RecordingId}: {Error}", recordingId, error));
    }
}
