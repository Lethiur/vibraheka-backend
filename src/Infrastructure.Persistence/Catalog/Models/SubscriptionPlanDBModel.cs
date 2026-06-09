using Amazon.DynamoDBv2.DataModel;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
namespace Infrastructure.Persistence.Catalog.Models;

[DynamoDBTable("SubscriptionPlans")]
public class SubscriptionPlanDBModel : ProductDBModel
{
    [DynamoDBHashKey]
    public string SubscriptionPlanID { get => ID; set => ID = value; }
    
    [DynamoDBProperty]
    public bool IncludesFullCatalog { get; set; }
    
}
