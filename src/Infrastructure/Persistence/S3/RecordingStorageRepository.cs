using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Recordings.Errors;
using VibraHeka.Domain.Recordings.Ports.Out;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Persistence.S3;

public class RecordingStorageRepository(IAmazonS3 client, AWSConfig options)
    : GenericS3Repository(client, options.RecordingsBucketName), IRecordingStoragePort
{
    private const int DownloadUrlExpirySeconds = 3600;

    public async Task<Result<string>> UploadAsync(
        string recordingId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), fileName);
        FileInfo fileInfo = await StreamToFile(fileStream, tempPath, cancellationToken);

        try
        {
            Result<string> result = await UploadAsync(fileInfo, recordingId, cancellationToken);
            return result;
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public Task<Result<string>> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken)
    {
        string s3ObjectKey = ExtractS3Key(storageKey);
        return GetDownloadPreSignedUrl(s3ObjectKey, DownloadUrlExpirySeconds);
    }

    private static string ExtractS3Key(string storageKey)
    {
        if (!Uri.TryCreate(storageKey, UriKind.Absolute, out Uri? uri))
        {
            return storageKey;
        }

        return uri.AbsolutePath.TrimStart('/');
    }
}
