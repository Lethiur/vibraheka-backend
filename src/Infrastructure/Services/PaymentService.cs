using Stripe;
using Stripe.BillingPortal;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Services;

public class PaymentService(StripeConfig stripeConfig) : IPaymentService
{
    public async Task<string> RegisterOrder(UserEntity payer, OrderEntity orderEntity)
    {
        StripeConfiguration.ApiKey = stripeConfig.SecretKey;
        
        CustomerService customerService = new();
        Customer customer = await customerService.CreateAsync(new CustomerCreateOptions()
        {
            Email = payer.Email,
            Metadata = new Dictionary<string, string>
            {
                { "userId", payer.Id }
            }
        });

        Stripe.Checkout.SessionCreateOptions options = new Stripe.Checkout.SessionCreateOptions()
        {
            Mode = "subscription",
            Customer = customer.Id,
            PaymentMethodTypes = ["card", "paypal", "revolut_pay"],
            LineItems =
            [
                new() { Price = orderEntity.ItemID, Quantity = orderEntity.Quantity }
            ],
            SuccessUrl = "https://example.com/success",
            CancelUrl = "https://example.com/cancel",
            ClientReferenceId = Guid.NewGuid().ToString()
        };
        
        Stripe.Checkout.SessionService sessionService = new Stripe.Checkout.SessionService();
        Stripe.Checkout.Session? session = await sessionService.CreateAsync(options);

        return session.Url;
    }

    public async Task<string> CreateUrlForPaymentDetail(UserEntity payer)
    {
        StripeConfiguration.ApiKey = stripeConfig.SecretKey;
        ;
        CustomerService customerService = new();
        Customer customer = await customerService.CreateAsync(new CustomerCreateOptions()
        {
            Email = payer.Email,
            Metadata = new Dictionary<string, string>
            {
                { "userId", payer.Id }
            }
        });
        SessionCreateOptions options = new SessionCreateOptions() { Customer = customer.Id};
        SessionService sessionService = new SessionService();
        Session? session = await sessionService.CreateAsync(options);

        return session.Url;
    }
}
