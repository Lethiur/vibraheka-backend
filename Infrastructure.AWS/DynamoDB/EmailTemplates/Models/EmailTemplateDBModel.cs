using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.EmailTemplates.Models;

[DynamoDBTable("EmailTemplates")]
public class EmailTemplateDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey("ID")]
    public string TemplateID { get; set; } = string.Empty;

    [DynamoDBProperty] 
    public string Path { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
    
    [DynamoDBProperty]
    public List<string> Attachments { get; set; } = [];
    
}
