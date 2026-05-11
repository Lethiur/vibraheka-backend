namespace Infrastructure.Rest.Client.Stripe.Enums;

public enum MissingPaymentMethodBehaviour
{
    Cancel,
    CreateInvoice,
    Pause
}
