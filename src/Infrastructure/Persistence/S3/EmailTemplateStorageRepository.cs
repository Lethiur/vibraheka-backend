using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Persistence.S3;

/// <summary>
/// A repository for managing email templates stored in an S3 bucket. This repository
/// provides methods to retrieve, save, delete, and authorize access to email templates.
/// </summary>
/// <remarks>
/// This class extends the <c>GenericS3Repository</c> to provide S3-specific functionality
/// for email template operations and implements the <c>IEmailTemplateStorageRepository</c>
/// interface.
/// </remarks>
public class EmailTemplateStorageRepository(IAmazonS3 client, AWSConfig options)
    : GenericS3Repository(client, options.EmailTemplatesBucketName), IEmailTemplateStorageRepository
{
    

    /// <summary>
    /// Retrieves the email template identified by the specified template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <returns>A result containing a stream of the template content if successful; otherwise, an error result.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<string>> GetTemplateUrlAsync(string templateID)
    {
        return GetDownloadPreSignedUrl($"{templateID}/template.json", 60);
    }

    /// <summary>
    /// Saves the provided email template to the storage repository identified by the specified template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier for the email template to save.</param>
    /// <param name="templateStream">The stream containing the content of the email template to be saved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A result indicating success or failure of the save operation.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public async Task<Result<string>> SaveTemplate(string templateID, Stream templateStream,
        CancellationToken cancellationToken)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), "template.json");
        FileInfo streamToFile = await StreamToFile(templateStream, tempPath, cancellationToken);
        Result<string> uploadAsync = await UploadAsync(streamToFile, templateID, cancellationToken);
        File.Delete(tempPath);
        return uploadAsync;
    }

    /// <summary>
    /// Checks if an email template exists in the storage for the specified template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to check for existence.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A result containing a boolean value indicating whether the template exists.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<bool>> TemplateExistsAsync(string templateID, CancellationToken cancellationToken)
    {
        return FileExistsAsync($"{templateID}/template.json", cancellationToken);
    }

    /// <summary>
    /// Retrieves the S3 path of the specified email template based on its template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template whose S3 path is to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the S3 path of the email template if successful; otherwise, an error result.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<string>> GetEmailTemplatePath(string templateID, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Saves an attachment associated with the specified email template ID into the storage repository.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to which the attachment belongs.</param>
    /// <param name="attachmentStream">The stream representing the attachment content to be saved.</param>
    /// <param name="attachmentName">The name of the attachment to be saved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the path of the saved attachment if successful; otherwise, an error result.</returns>
    /// <exception cref="IOException">Thrown if an I/O error occurs during file operations.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if permission to access the filesystem or cloud storage is denied.</exception>
    public async Task<Result<string>> SaveAttachment(string templateID, Stream attachmentStream, string attachmentName,
        CancellationToken cancellationToken)
    {
        string tempPath = Path.Combine(Path.GetTempPath(), attachmentName);
        FileInfo info = await StreamToFile(attachmentStream, attachmentName, cancellationToken);
        try 
        {
            Result<string> uploadAsync =
                await UploadAsync(info, $"{templateID}/attachments", cancellationToken);
            return uploadAsync;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        
    }

    /// <summary>
    /// Generates an authorization string that allows read access to the specified email template.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template for which the authorization string is being generated.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the authorization string if successful; otherwise, an error result.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<string>> GetAuthorizationStringForTemplateRead(string templateID,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves an authorization string required to perform write operations on the specified email template.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template for which the authorization string is generated.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the request.</param>
    /// <returns>A result containing the authorization string if successful; otherwise, an error result.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<string>> GetAuthorizationStringForTemplateWrite(string templateID,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Deletes the email template identified by the specified template ID from the storage repository.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result indicating success if the template was deleted; otherwise, an error result.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<Unit>> DeleteTemplate(string templateID, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves the content of an email template identified by the specified template ID.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the email template content as a string if successful; otherwise, an error result.</returns>
    /// <exception cref="NotImplementedException">Thrown when this method is not implemented.</exception>
    public Task<Result<string>> GetTemplateContent(string templateID, CancellationToken cancellationToken)
    {
        return GetFileContents($"{templateID}/template.json", cancellationToken);
    }
}
