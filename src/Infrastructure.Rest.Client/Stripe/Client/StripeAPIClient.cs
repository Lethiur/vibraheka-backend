using CSharpFunctionalExtensions;
using Infrastructure.Rest.Client.Stripe.Enums;
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

    /// <summary>
    /// Creates a new product and associated price in the Stripe system.
    /// </summary>
    /// <param name="request">The details required for product and price creation, including product name, description, currency, price in cents, and metadata.</param>
    /// <param name="token">A token to observe cancellation requests.</param>
    /// <returns>A result object containing the created product ID and price ID if the operation is successful; otherwise, an error message.</returns>
    public async Task<Result<CreateProductAndPriceResponse>> CreateProductAndPriceAsync(
        CreateProductAndPriceRequest request,
        CancellationToken token)
    {
        try
        {
            ProductService productService = new();
            ProductCreateOptions productCreateOptions = new ProductCreateOptions()
            {
                Name = request.Name, Description = request.Description, Metadata = request.Metadata
            };

            Product product = await productService.CreateAsync(productCreateOptions, cancellationToken: token);
            CreatePriceRequest priceCreateOptions = new()
            {
                Currency = request.Currency,
                Metadata = request.Metadata,
                PriceInCents = request.PriceInCents,
                ProductID = product.Id
            };
            
            return await AddPriceToProduct(priceCreateOptions, token)
                .Map(priceID => new CreateProductAndPriceResponse() { ProductID = product.Id, PriceID = priceID });
            
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating product and price");
            return Result.Failure<CreateProductAndPriceResponse>(StripeErrors.FailedToCreateProductAndPrice);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating product and price");
            return Result.Failure<CreateProductAndPriceResponse>(StripeErrors.FailedToCreateProductAndPrice);
        }
    }

    /// <summary>
    /// Adds a price to an existing product in the Stripe system and returns the ID of the created price.
    /// </summary>
    /// <param name="request">The details of the price to be added, including product ID, price in cents, currency, metadata, and optional recurring payment options.</param>
    /// <param name="token">A token to observe cancellation requests.</param>
    /// <returns>A result object containing the Stripe price ID if the operation is successful; otherwise, an error message or failure result.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported recurring payment option is provided in the request.</exception>
    public async Task<Result<string>> AddPriceToProduct(CreatePriceRequest request, CancellationToken token)
    {
        PriceService priceService = new();

        PriceCreateOptions priceCreateOptions = new()
        {
            Currency = request.Currency,
            Metadata = request.Metadata,
            Active = true,
            UnitAmount = request.PriceInCents,
            Product = request.ProductID
        };

        if (request.PaymentRecurringOptions != null)
        {
            priceCreateOptions.Recurring = new PriceRecurringOptions()
            {
                Interval = request.PaymentRecurringOptions switch
                {
                    PaymentRecurringOptions.Monthly => "month",
                    PaymentRecurringOptions.Yearly => "year",
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }
        Price price = await priceService.CreateAsync(priceCreateOptions, cancellationToken: token);
        return price.Id;
    }

    /// <summary>
    /// Initiates a Stripe Checkout session for a subscription order and returns the result containing the session's details.
    /// </summary>
    /// <param name="orderRequest">The subscription order details, including customer information, trial period settings, payment method collection preferences, and behavior when no payment method is provided.</param>
    /// <param name="token">A token that allows the operation to be canceled.</param>
    /// <returns>A result object containing the details of the created Stripe Checkout session if the process is successful; otherwise, an error message.</returns>
    public Task<Result<CheckoutResult>> CheckoutSubscriptionAsync(StartSubscriptionOrderRequest orderRequest,
        CancellationToken token)
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
    private async Task<Result<CheckoutResult>> PerformCheckoutAsync(SessionCreateOptions options,
        CancellationToken token)
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
        List<SessionLineItemOptions> sessionLineItemListOptions = order.OrderLines.Select(line =>
                new SessionLineItemOptions()
                    {
                        Price = line.PriceRef, Quantity = line.Quantity, Metadata = line.Metadata
                    })
            .ToList();

        return new SessionCreateOptions
        {
            Mode = "payment",
            Customer = order.CustomerID,
            PaymentMethodTypes = order.PaymentMethodsAccepted,
            LineItems = sessionLineItemListOptions,
            SuccessUrl = order.SuccessCallbackUrl,
            CancelUrl = order.FailureCallbackUrl,
            ClientReferenceId = order.OrderID,
            ExpiresAt = DateTime.UtcNow.AddHours(23),
            Metadata = order.Metadata
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
