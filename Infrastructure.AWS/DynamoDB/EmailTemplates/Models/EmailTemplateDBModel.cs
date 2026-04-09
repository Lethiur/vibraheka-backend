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
    
    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
    
}
