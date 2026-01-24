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
    /// <param name="uploadPath">The path on the S3</param>
    /// <param name="token">The token used to cancel the task</param>
    /// <returns>Whether the file was properly uploaded to the bucket</returns>
    protected async Task<Result<string>> UploadAsync(FileInfo file, string uploadPath, CancellationToken token)
    {
        if (file is not { Exists: true })
        {
            return  Result.Failure<string>("File does not exist");
        }

        await using Stream strean = file.OpenRead();
        PutObjectRequest objectRequest = new() { BucketName = BucketName, Key = uploadPath + "/" + file.Name, InputStream = strean };
        objectRequest.Metadata.Add("Content-Type", "image/png");

        PutObjectResponse objectAsync = await Client.PutObjectAsync(objectRequest, token);

        return objectAsync.HttpStatusCode == HttpStatusCode.OK
            ? Result.Success(
                $"https://{BucketName}.s3.{Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{uploadPath}/{file.Name}")
            : Result.Failure<string>("Failed to upload file");
    }


    /// <summary>
    /// Checks asynchronously if a file exists in the specified S3 bucket.
    /// </summary>
    /// <param name="fileKey">The key of the file to check for existence on S3.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the task.</param>
    /// <returns>A result indicating whether the file exists in the bucket or not.</returns>
    protected async Task<Result<bool>> FileExistsAsync(string fileKey, CancellationToken cancellationToken)
    {
        try
        {
            await Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = BucketName,
                Key = fileKey
            }, cancellationToken);

            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
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
    /// Writes the contents of a stream to a file at the specified file path.
    /// </summary>
    /// <param name="stream">The input stream containing data to be written to the file.</param>
    /// <param name="filePath">The path where the file will be created or overwritten.</param>
    /// <param name="cancellationToken">Token used to listen for task cancellation.</param>
    /// <returns>The created FileInfo object representing the newly created file.</returns>
    protected async Task<FileInfo> StreamToFile(Stream stream, string filePath, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
        
        await using (FileStream file = new(
                         filePath,
                         FileMode.Create,
                         FileAccess.Write,
                         FileShare.None))
        {
            await stream.CopyToAsync(file, cancellationToken);
            await file.FlushAsync(cancellationToken);
        }
        
        return new FileInfo(filePath);
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

    /// <summary>
    /// Generates a pre-signed URL for downloading a file from an S3 bucket.
    /// </summary>
    /// <param name="key">The unique identifier of the file in the S3 bucket.</param>
    /// <param name="expiresInSeconds">The duration in seconds for which the generated URL will remain valid.</param>
    /// <returns>A result containing the pre-signed URL if successful, or an error message if the operation fails.</returns>
    protected async Task<Result<string>> GetDownloadPreSignedUrl(string key, int expiresInSeconds)
    {
        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
            Verb = HttpVerb.GET,
            
        };

        return await Client.GetPreSignedURLAsync(request);
    }
}
