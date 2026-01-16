using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

[DynamoDBTable("EmailTemplates")]
public class EmailTemplateDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey("ID")]
    public string TemplateID { get; set; } = string.Empty;

    [DynamoDBProperty] 
    public string Path { get; set; } = string.Empty;
    
    
    public EmailEntity ToDomain() => new()
    {
        ID = TemplateID,
        Path = Path,
        Created = Created,
        CreatedBy = CreatedBy,
        LastModified = LastModified,
        LastModifiedBy = LastModifiedBy
    };
    
    public static EmailTemplateDBModel FromDomain(EmailEntity entity) => new()
    {
        TemplateID = entity.ID,
        Path = entity.Path,
        Created = entity.Created,
        CreatedBy = entity.CreatedBy,
        LastModified = entity.LastModified,
        LastModifiedBy = entity.LastModifiedBy
    };
    
}
