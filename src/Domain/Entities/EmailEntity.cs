namespace VibraHeka.Domain.Entities;

/// <summary>
/// Represents an email entity with identification and path properties.
/// Includes auditing information inherited from <see cref="BaseAuditableEntity"/>.
/// </summary>
public class EmailEntity : BaseAuditableEntity
{
    public string ID { get; set; } = string.Empty;
    
    public string Path { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public List<string> Attachments { get; set; } = [];
    
}
 
