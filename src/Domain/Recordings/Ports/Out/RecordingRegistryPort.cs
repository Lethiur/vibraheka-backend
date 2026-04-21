using CSharpFunctionalExtensions;
using VibraHeka.Domain.Recordings.Entities;

namespace VibraHeka.Domain.Recordings.Ports.Out;

public interface IRecordingRegistryPort
{
    Task<Result<string>> SaveRecording(RecordingEntity recording, CancellationToken cancellationToken);
    Task<Result<IEnumerable<RecordingEntity>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<RecordingEntity>> GetByIdAsync(string recordingId, CancellationToken cancellationToken);
    Task<Result> DeleteRecordingAsync(RecordingEntity recording, CancellationToken cancellationToken);
}
