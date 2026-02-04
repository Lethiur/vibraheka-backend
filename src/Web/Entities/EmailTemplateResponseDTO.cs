namespace VibraHeka.Web.Entities;

public class EmailTemplateResponseDTO
{
    public string TemplateID { get; set; } = string.Empty;
    
    public string TemplateName { get; set; } = string.Empty;
    
    public DateTimeOffset LastModified { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } 
}
