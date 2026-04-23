using CSharpFunctionalExtensions;

namespace VibraHeka.Domain.Recordings.Ports.Out;

public interface IRecordingStoragePort
{
    Task<Result<string>> GetUploadUrlAsync(string storageKey, CancellationToken cancellationToken);
    Task<Result<string>> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken);
    Task<Result> DeleteFileAsync(string storageKey, CancellationToken cancellationToken);
}
