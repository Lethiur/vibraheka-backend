using CSharpFunctionalExtensions;
using Infrastructure.Persistence.Catalog.Repositories;
using MediatR;
using VibraHeka.Domain.Catalog.Entities;
using VibraHeka.Domain.Catalog.Ports.Out;

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

    public Task<Result<Unit>> DeactivateRecordingAsync(string recordingId, CancellationToken cancellationToken)
    {
        return repository.DeactivateRecording(recordingId, cancellationToken);
    }

    public Task<Result<Unit>> ActivateRecordingAsync(string recordingId, CancellationToken cancellationToken)
    {
        return repository.ActivateRecording(recordingId, cancellationToken);
    }
}
