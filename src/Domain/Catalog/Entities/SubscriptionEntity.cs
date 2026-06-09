namespace VibraHeka.Domain.Catalog.Entities;

public class SubscriptionEntity : ProductEntity
{
    public string SubscriptionID
    {
        get => ID; set => ID = value;
    }
    
    public string UserID {get; set;} = string.Empty;
    
    public string PaymentGatewayUserID { get;set;} = string.Empty;
    
    public DateTimeOffset CurrentPeriodStart { get; set; }
    public DateTimeOffset CurrentPeriodEnd { get; set; }
    
    public bool cancelAtPeriodEnd { get; set; }
    
}
