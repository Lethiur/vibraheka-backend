using System.Diagnostics.CodeAnalysis;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.S3;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class GenericS3Repository(IAmazonS3 client, string bucketName)
{
     /// <summary>
    /// Client used to communicate with AWS
    /// </summary>
    private readonly IAmazonS3 Client = client;

    /// <summary>
    /// The name of the bucket to operate with
    /// </summary>
    private readonly string BucketName = bucketName;

    /// <summary>
    /// Sends a file to AWS on async fashion
    /// </summary>
    /// <param name="file">The file to be sent</param>
    /// <param name="token">The token used to cancel the task</param>
    /// <returns>Whether the file was properly uploaded to the bucket</returns>
    protected async Task<Result<bool>> UploadAsync(FileInfo file, CancellationToken token)
    {
        bool ret = false;
        if (file is { Exists: true })
        {
            // making a TransferUtilityUploadRequest instance
            PutObjectRequest objectRequest = new()
            {
                BucketName = BucketName,
                Key = file.Name,
                InputStream = file.OpenRead()
            };

            objectRequest.Metadata.Add("Content-Type", "image/png");

            PutObjectResponse objectAsync = await Client.PutObjectAsync(objectRequest, token);
            ret = objectAsync.HttpStatusCode == HttpStatusCode.OK;    
        }

        return ret;
    }

    /// <summary>
    /// Retrieves the contents of a file inside the bucket using its file key
    /// </summary>
    /// <param name="fileKey">The file key to retrieve</param>
    /// <param name="token">Token used to listen for task cancellation</param>
    /// <returns>contents of the file on string form</returns>
    protected async Task<Result<string>> GetFileContents(string fileKey, CancellationToken token)
    {
        GetObjectResponse getObjectResponse =
            await Client.GetObjectAsync(BucketName, fileKey, token);
        
        await using Stream responseStream = getObjectResponse.ResponseStream;
        using StreamReader reader = new(responseStream);

        string content = await reader.ReadToEndAsync(token); // Reads the content as a string
        return Result.Success(content);
    }

    /// <summary>
    /// Retrieves a pre-signed URL to upload a file from a
    /// different client to the bucket this client
    /// is managing 
    /// </summary>
    /// <param name="key">The name of the file</param>
    /// <param name="expiresInSeconds">The amount of time to initiate the upload</param>
    /// <param name="md5Hash">The hash of the file</param>
    /// <returns>A <see cref="Result"/> containing the pre-signed url or an error otherwise</returns>
    protected async Task<Result<string>> GetUploadPreSignedUrl(string key, int expiresInSeconds, string md5Hash)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Result.Failure<string>(InfrastructureFileManagementErrors.InvalidKey);
        }

        if (expiresInSeconds < 0)
        {
            return Result.Failure<string>(InfrastructureFileManagementErrors.InvalidExpiryDate);
        }

        if (string.IsNullOrEmpty(md5Hash))
        {
            return Result.Failure<string>(InfrastructureFileManagementErrors.InvalidHash);
        }
        
        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            Verb = HttpVerb.PUT,
            Headers = { ContentMD5 = md5Hash },
            ContentType = "application/octet-stream",
            
        };
        
        return await Client.GetPreSignedURLAsync(request);
    }
}
