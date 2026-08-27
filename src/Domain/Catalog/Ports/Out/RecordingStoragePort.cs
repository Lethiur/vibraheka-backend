using CSharpFunctionalExtensions;

namespace VibraHeka.Domain.Catalog.Ports.Out;

public interface IRecordingStoragePort
{
    Task<Result<string>> GetUploadUrlAsync(string storageKey, CancellationToken cancellationToken);
    Task<Result<string>> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken);
    Task<Result> DeleteFileAsync(string storageKey, CancellationToken cancellationToken);
}
