namespace Infrastructure.Rest.Client.Stripe.Errors;

public static class StripeErrors
{
    public const string FailedToCreateCheckoutSession = "S-001";
    public const string FailedToCreateCustomer = "S-002";
    public const string FailedToCreateProductAndPrice = "S-003";
}
