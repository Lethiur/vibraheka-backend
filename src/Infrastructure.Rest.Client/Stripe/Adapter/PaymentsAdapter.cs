using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Client;
using Infrastructure.Rest.Client.Stripe.Mappers;
using Infrastructure.Rest.Client.Stripe.Models;
using VibraHeka.Domain.Commerce.Entities;
using VibraHeka.Domain.Commerce.Models;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Ports.Out;

namespace Infrastructure.Rest.Client.Stripe.Adapter;

public class PaymentsAdapter(StripeAPIClient StripeApiClient, StripeMapper stripeMapper) : IPaymentsPort
{
    public Task<Result<string>> RegisterCustomerAsync(UserEntity user, CancellationToken token)
    {
        RegisterCustomerRequest request = stripeMapper.FromUserEntityToRegisterCustomerRequest(user);
        return StripeApiClient.RegisterCustomerAsync(request, token);
    }

    public Task<Result<PaymentAttemptEntity>> CreatePaymentIntentAsync(OrderEntity order, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<CheckoutSessionCompletedModel>> CreateCheckoutSessionAsync(CheckoutProductModel model,
        CancellationToken token)
    {
        StartOrderRequest request = stripeMapper.FromDomainToStartOrderRequest(model, ["card", "paypal", "klarna"]);
        Result<CheckoutResult> checkoutProductAsync = await StripeApiClient.CheckoutProductAsync(request, token);
        return checkoutProductAsync.Map(stripeMapper.FromCheckoutResultToCheckoutSessionCompletedModel);
    }
}
