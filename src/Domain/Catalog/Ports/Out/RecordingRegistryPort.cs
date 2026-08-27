using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface IRecordingRegistryPort
{
    Task<Result<string>> SaveRecording(RecordingEntity recording, CancellationToken cancellationToken);
    Task<Result<IEnumerable<RecordingEntity>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<RecordingEntity>> GetByIdAsync(string recordingId, CancellationToken cancellationToken);
    Task<Result> DeleteRecordingAsync(RecordingEntity recording, CancellationToken cancellationToken);
    
    Task<Result<Unit>> DeactivateRecordingAsync(string recordingId, CancellationToken cancellationToken);
    
    Task<Result<Unit>> ActivateRecordingAsync(string recordingId, CancellationToken cancellationToken);
}
