namespace VibraHeka.Web.Entities;

/// <summary>
/// Represents a request to edit the name of an existing template.
/// </summary>
public class EditTemplateNameRequest
{
    public string TemplateID { get; set; } = string.Empty;
    public string NewTemplateName { get; set; } = string.Empty;
}
