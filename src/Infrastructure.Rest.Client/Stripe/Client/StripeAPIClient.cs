using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Errors;
using Infrastructure.Rest.Client.Stripe.Models;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Rest.Client.Stripe.Client;

public class StripeAPIClient(ILogger<StripeAPIClient> logger)
{
    public async Task<Result<string>> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken token)
    {
        try
        {
            logger.LogInformation("Registering new customer with email {Email} and userID {UserID}", request.Email,
                request.UserID);
            CustomerService customerService = new();
            Customer customer = await customerService.CreateAsync(
                new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = $"{request.FirstName} {request.LastName}",
                    Phone = request.PhoneNumber,
                    Metadata = new Dictionary<string, string> { { "UserID", request.UserID } }
                }, cancellationToken: token);

            return customer.Id;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating the customer");
            return Result.Failure<string>(StripeErrors.FailedToCreateCustomer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating the customer");
            return Result.Failure<string>(StripeErrors.FailedToCreateCustomer);
        }
    }

    public Task<Result<CheckoutResult>> CheckoutSubscription(StartSubscriptionOrder order, CancellationToken token)
    {
        SessionCreateOptions options = CreateCheckoutSubscriptionOptions(order);
        return PerformCheckout(options, token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Result<CheckoutResult>> CheckoutProduct(StartOrderRequest order, CancellationToken token)
    {
        SessionCreateOptions options = CreateCheckoutProductOptions(order);
        return PerformCheckout(options, token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<Result<CheckoutResult>> PerformCheckout(SessionCreateOptions options, CancellationToken token)
    {
        try
        {
            SessionService sessionService = new();
            Session? session = await sessionService.CreateAsync(options, cancellationToken: token);

            if (session != null)
            {
                return new CheckoutResult
                {
                    ExpiresAt = DateTime.UtcNow.AddHours(23),
                    InternalPaymentID = session.ClientReferenceId,
                    PaymentSessionID = session.Id,
                    Url = session.Url,
                };
            }

            logger.LogError("Failed to create checkout session for order {OrderID} and customer {CustomerID}",
                options.ClientReferenceId, options.Customer);
            return Result.Failure<CheckoutResult>(StripeErrors.FailedToCreateCheckoutSession);
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating checkout session");
            return Result.Failure<CheckoutResult>(StripeErrors.FailedToCreateCheckoutSession);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error while creating checkout session for order {OrderID} and customer {CustomerID}",
                options.ClientReferenceId, options.Customer);
            return Result.Failure<CheckoutResult>(StripeErrors.FailedToCreateCheckoutSession);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private static SessionCreateOptions CreateCheckoutProductOptions(StartOrderRequest order)
    {
        return new SessionCreateOptions
        {
            Mode = "payment",
            Customer = order.CustomerID,
            PaymentMethodTypes = order.PaymentMethodsAccepted,
            LineItems =
            [
                new SessionLineItemOptions { Price = order.ProductRef, Quantity = order.OrderQuantity }
            ],
            SuccessUrl = order.SuccessCallbackUrl,
            CancelUrl = order.FailureCallbackUrl,
            ClientReferenceId = order.OrderID,
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };
    }


    private static SessionCreateOptions CreateCheckoutSubscriptionOptions(StartSubscriptionOrder order)
    {
        SessionCreateOptions options = CreateCheckoutProductOptions(order);
        options.Mode = "subscription";
        options.PaymentMethodCollection = "always";
        options.SubscriptionData = new SessionSubscriptionDataOptions()
        {
            TrialSettings = new SessionSubscriptionDataTrialSettingsOptions()
            {
                EndBehavior = new SessionSubscriptionDataTrialSettingsEndBehaviorOptions()
                {
                    MissingPaymentMethod = "cancel"
                }
            },
            TrialPeriodDays = order.TrialPeriodDays,
        };
        return options;
    }
}
