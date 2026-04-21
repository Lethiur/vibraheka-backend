using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;

namespace VibraHeka.Application.Recordings.Commnads.AdminAddRecording;

public class AdminAddRecordingCommandHandler(
    IRecordingStoragePort StoragePort,
    IRecordingRegistryPort RegistryPort,
    ICurrentUserService CurrentUserService)
    : IRequestHandler<AdminAddRecordingCommand, Result<string>>
{
    public Task<Result<string>> Handle(AdminAddRecordingCommand request, CancellationToken cancellationToken)
    {
        string recordingId = Guid.NewGuid().ToString();

        return StoragePort
            .UploadAsync(recordingId, request.FileStream, request.FileName, cancellationToken)
            .Bind(storageKey =>
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
            });
    }
}
