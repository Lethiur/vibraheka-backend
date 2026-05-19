using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Client;
using Infrastructure.Rest.Client.Stripe.Mappers;
using Infrastructure.Rest.Client.Stripe.Models;
using VibraHeka.Application.Payments.Models;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Entities;
using VibraHeka.Domain.Payments.Enums;
using VibraHeka.Domain.Payments.Ports.Out;

namespace Infrastructure.Rest.Client.Stripe.Adapter;

public class PaymentsAdapter(StripeAPIClient StripeApiClient, StripeMapper stripeMapper) : IPaymentsPort
{
    public Task<Result<string>> RegisterCustomerAsync(ref readonly UserEntity user, CancellationToken token)
    {
        RegisterCustomerRequest request = stripeMapper.FromUserEntityToRegisterCustomerRequest(user);
        return StripeApiClient.RegisterCustomerAsync(request, token);
    }

    public async Task<Result<PaymentAttemptEntity>> StartPaymentProcessAsync(CheckoutOrderModel checkoutModel,
        CancellationToken token)
    {
        
        List<OrderLine> orderLines = checkoutModel.Order.Lines.Select(line => new OrderLine()
        {
            Quantity = line.Quantity,
            PriceRef = line.PaymentGatewayPriceIDSnapshot,
            Metadata = new Dictionary<string, string>()
            {
                {"SellableItemId", line.SellableItemID},
                {"SellableItemPriceId", line.SellablePriceID},
            }
        }).ToList();

        StartOrderRequest request = new StartOrderRequest()
        {
            CustomerID = checkoutModel.CustomerID,
            OrderID = checkoutModel.Order.OrderID,
            FailureCallbackUrl = checkoutModel.CancelCallbackUrl,
            SuccessCallbackUrl = checkoutModel.SuccessCallbackUrl,
            PaymentMethodsAccepted = checkoutModel.PaymentMethodsAccepted,
            OrderLines = orderLines,
            Metadata = new Dictionary<string, string>()
            {
                {"OrderID", checkoutModel.Order.OrderID},
                {"UserID", checkoutModel.CustomerID},
            }
        };

        (bool _, bool isFailure, CheckoutResult value, string error) = await StripeApiClient.CheckoutProductAsync(request, token);

        if (isFailure)
        {
            return Result.Failure<PaymentAttemptEntity>(error);
        }

        PaymentAttemptEntity attempt = new PaymentAttemptEntity
        {
            PaymentGatewayCheckoutURL = value.Url, 
            ExpiresAt = value.ExpiresAt, 
            PaymentGatewayIntentID = value.InternalPaymentID,
            PaymentGatewayCheckoutSessionID = value.InternalPaymentID,
            Provider = PaymentsProviders.Stripe
        };

        return Result.Success<PaymentAttemptEntity>(attempt);
    }
}
