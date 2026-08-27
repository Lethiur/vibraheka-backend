using Amazon.S3;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Catalog.Errors;
using VibraHeka.Domain.Catalog.Ports.Out;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Persistence.S3;

public class RecordingStorageRepository(IAmazonS3 client, AWSConfig options)
    : GenericS3Repository(client, options.RecordingsBucketName), IRecordingStoragePort
{
    private const int DownloadUrlExpirySeconds = 3600;
    private const int UploadUrlExpirySeconds = 900;

    public Task<Result<string>> GetUploadUrlAsync(string storageKey, CancellationToken cancellationToken)
        => GetUploadPreSignedUrl(storageKey, UploadUrlExpirySeconds)
            .Map(url => url)
            .MapError(_ => RecordingErrors.UrlGenerationFailed);

    public Task<Result<string>> GetDownloadUrlAsync(string storageKey, CancellationToken cancellationToken)
    {
        string s3ObjectKey = ExtractS3Key(storageKey);
        return GetDownloadPreSignedUrl(s3ObjectKey, DownloadUrlExpirySeconds);
    }

    public async Task<Result> DeleteFileAsync(string storageKey, CancellationToken cancellationToken)
    {
        string s3Key = ExtractS3Key(storageKey);
        Result<Unit> result = await DeleteObjectAsync(s3Key, cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
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
