namespace VibraHeka.Web.Entities;

public class UploadEmailTemplateAttachment
{
    public string AttachmentName { get; set; } = string.Empty;
    
    public string TemplateID { get; set; } = string.Empty;
    
    public IFormFile File { get; set; } = null!;
}
