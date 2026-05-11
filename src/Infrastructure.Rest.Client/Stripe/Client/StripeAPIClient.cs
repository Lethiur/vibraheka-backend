using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Errors;
using Infrastructure.Rest.Client.Stripe.Models;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Rest.Client.Stripe.Client;

public class StripeAPIClient(ILogger<StripeAPIClient> logger)
{
    /// <summary>
    /// Registers a new customer in the Stripe system and returns the created customer ID.
    /// </summary>
    /// <param name="request">The customer details required for registration, including email, name, phone number, and user ID.</param>
    /// <param name="token">A token to observe cancellation requests.</param>
    /// <returns>A result object containing the Stripe customer ID if the registration is successful; otherwise, an error message.</returns>
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

    public Task<Result<CheckoutResult>> CheckoutSubscriptionAsync(StartSubscriptionOrderRequest orderRequest, CancellationToken token)
    {
        SessionCreateOptions options = CreateCheckoutSubscriptionOptions(orderRequest);
        return PerformCheckoutAsync(options, token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<Result<CheckoutResult>> CheckoutProductAsync(StartOrderRequest order, CancellationToken token)
    {
        SessionCreateOptions options = CreateCheckoutProductOptions(order);
        return PerformCheckoutAsync(options, token);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<Result<CheckoutResult>> PerformCheckoutAsync(SessionCreateOptions options, CancellationToken token)
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
    /// Creates session options for a Stripe Checkout process for a product order.
    /// </summary>
    /// <param name="order">The details of the product order, including customer information, payment methods, and callback URLs.</param>
    /// <returns>A <see cref="SessionCreateOptions"/> object populated with the details required for initiating a Stripe Checkout session.</returns>
    private static SessionCreateOptions CreateCheckoutProductOptions(StartOrderRequest order)
    {
        return new SessionCreateOptions
        {
            Mode = "payment",
            Customer = order.CustomerID,
            PaymentMethodTypes = order.PaymentMethodsAccepted,
            LineItems =
            [
                new SessionLineItemOptions { Price = order.PriceRef, Quantity = order.OrderQuantity }
            ],
            SuccessUrl = order.SuccessCallbackUrl,
            CancelUrl = order.FailureCallbackUrl,
            ClientReferenceId = order.OrderID,
            ExpiresAt = DateTime.UtcNow.AddHours(23)
        };
    }

    /// <summary>
    /// Creates session options for a Stripe Checkout process for a subscription order.
    /// </summary>
    /// <param name="orderRequest">The details of the subscription order, including customer information, payment methods, trial settings, and callback URLs.</param>
    /// <returns>A <see cref="SessionCreateOptions"/> object populated with the necessary details for initiating a Stripe Checkout session for a subscription.</returns>
    private static SessionCreateOptions CreateCheckoutSubscriptionOptions(StartSubscriptionOrderRequest orderRequest)
    {
        SessionCreateOptions options = CreateCheckoutProductOptions(orderRequest);
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
            TrialPeriodDays = orderRequest.TrialPeriodDays,
        };
        return options;
    }
}
