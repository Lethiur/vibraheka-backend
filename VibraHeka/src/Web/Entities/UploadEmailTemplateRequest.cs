namespace VibraHeka.Web.Entities;

public class UploadEmailTemplateRequest
{
    public string TemplateName { get; set; } = string.Empty;
    
    public IFormFile File { get; set; } = null!;
}
