using CSharpFunctionalExtensions;
using VibraHeka.Domain.Recordings.Entities;

namespace VibraHeka.Domain.Recordings.Ports.Out;

public interface IRecordingRegistryPort
{
    Task<Result<string>> SaveAsync(RecordingEntity recording, CancellationToken cancellationToken);
}
