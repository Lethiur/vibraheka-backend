namespace VibraHeka.Web.Entities;

/// <summary>
/// Represents a request to upload a new email template.
/// Contains the required details for the operation, including the template name and the file to be uploaded.
/// </summary>
public class UploadEmailTemplateRequest
{
    /// <summary>
    /// Gets or sets the name of the email template.
    /// This property represents the unique name assigned to the email template,
    /// which is required for identification and storage.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file associated with the email template upload request.
    /// This property contains the file to be uploaded, typically in the form of a multipart/form-data stream.
    /// </summary>
    public IFormFile File { get; set; } = null!;
}
