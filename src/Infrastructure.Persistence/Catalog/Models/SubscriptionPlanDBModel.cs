using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("SubscriptionPlans")]
public class SubscriptionPlanDBModel : BaseAuditableDBModel
{
    [DynamoDBHashKey]
    public string SubscriptionPlanID { get; set; } = string.Empty;
    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
    [DynamoDBProperty]
    public bool IncludesFullCatalog { get; set; }
    [DynamoDBProperty]
    public bool IsActive { get; set; }
}
