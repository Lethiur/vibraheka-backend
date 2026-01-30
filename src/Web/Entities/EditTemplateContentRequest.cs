namespace VibraHeka.Web.Entities;

/// <summary>
/// Represents a request to edit the content of a template.
/// This class contains the necessary data to update a specific template, including its identifier and the new content.
/// </summary>
public class EditTemplateContentRequest
{
    public string TemplateID { get; set; } = string.Empty;
    public IFormFile TemplateFile { get; set; } = null!;
}
