using CSharpFunctionalExtensions;

namespace VibraHeka.Domain.Recordings.Ports.Out;

public interface IRecordingStoragePort
{
    Task<Result<string>> UploadAsync(string recordingId, Stream fileStream, string fileName, CancellationToken cancellationToken);
    Task<Result<string>> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken);
    Task<Result> DeleteFileAsync(string storageKey, CancellationToken cancellationToken);
}
