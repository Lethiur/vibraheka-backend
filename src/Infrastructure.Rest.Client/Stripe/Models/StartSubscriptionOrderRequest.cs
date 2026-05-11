using Infrastructure.Rest.Client.Stripe.Enums;

namespace Infrastructure.Rest.Client.Stripe.Models;

public class StartSubscriptionOrderRequest : StartOrderRequest
{
    public int? TrialPeriodDays { get; set; }
    public MissingPaymentMethodBehaviour behaviourIfNoPaymentMethod { get; set; }
    public PaymentMethodCollection PaymentMethodCollection { get; set; }
}
