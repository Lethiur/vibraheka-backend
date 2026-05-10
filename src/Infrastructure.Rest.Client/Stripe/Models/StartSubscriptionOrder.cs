using Infrastructure.Rest.Client.Stripe.Enums;

namespace Infrastructure.Rest.Client.Stripe.Models;

public class StartSubscriptionOrder : StartOrderRequest
{
    public int? TrialPeriodDays { get; set; }
    public MissingPaymentMethodBehaviour behaviourIfNoPaymentMethod { get; set; }
}
