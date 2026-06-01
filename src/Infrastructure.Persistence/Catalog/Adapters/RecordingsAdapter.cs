using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using VibraHeka.Domain.Recordings.Entities;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.Persistence.Catalog.Adapters;

public class RecordingsAdapter(IRecordingRepository repository) : IRecordingRegistryPort
{
    public Task<Result<string>> SaveRecording(RecordingEntity recording, CancellationToken cancellationToken)
    {
        return repository.SaveRecording(recording, cancellationToken);
    }

    public Task<Result<IEnumerable<RecordingEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return repository.GetAllAsync(cancellationToken);
    }

    public Task<Result<RecordingEntity>> GetByIdAsync(string recordingId, CancellationToken cancellationToken)
    {
        return repository.GetByIdAsync(recordingId, cancellationToken);
    }

    public Task<Result> DeleteRecordingAsync(RecordingEntity recording, CancellationToken cancellationToken)
    {
        return repository.DeleteRecordingAsync(recording, cancellationToken);
    }
}
