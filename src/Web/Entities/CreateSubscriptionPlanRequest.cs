using VibraHeka.Domain.Catalog.Enums;

namespace VibraHeka.Web.Entities;

public class CreateSubscriptionPlanRequest : CreateProductRequest
{
    public BillingInterval BillingInterval { get; set; }
}
