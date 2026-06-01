using CSharpFunctionalExtensions;
using VibraHeka.Domain.Recordings.Entities;

namespace Infrastructure.Persistence.Catalog.Repositories;

public interface IRecordingRepository
{
    Task<Result<string>> SaveRecording(RecordingEntity recording, CancellationToken cancellationToken);

    Task<Result<IEnumerable<RecordingEntity>>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<RecordingEntity>> GetByIdAsync(string recordingId, CancellationToken cancellationToken);

    Task<Result> DeleteRecordingAsync(RecordingEntity recording, CancellationToken cancellationToken);
}
