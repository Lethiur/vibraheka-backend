using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Subscriptions.Entities;
using VibraHeka.Domain.Subscriptions.Ports.Out;
using VibraHeka.Infrastructure.Entities;
using BillingPortal = Stripe.BillingPortal;
using Checkout = Stripe.Checkout;

namespace Infrastructure.Stripe.Subscriptions.Adapters;

public class PaymentsAdapter(IOptions<StripeConfig> config, ILogger<PaymentsAdapter> logger) : PaymentsPort
{
    /// <summary>
    /// Initiates a subscription payment session with the payment gateway.
    /// </summary>
    /// <param name="gatewayCustomerID">
    /// The unique identifier for the customer in the payment gateway.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe cancellation requests.
    /// </param>
    /// <returns>
    /// A result object containing the subscription checkout session entity on success,
    /// or an error result on failure.
    /// </returns>
    public async Task<Result<SubscriptionCheckoutSessionEntity>> InitiateSubscriptionPaymentAsync(
        string gatewayCustomerID,
        CancellationToken cancellationToken)
    {
        try
        {
            DateTime expirationDate = DateTime.UtcNow.AddHours(23);
            SessionCreateOptions options = new()
            {
                Mode = "subscription",
                Customer = gatewayCustomerID,
                PaymentMethodTypes = config.Value.PaymentMethodsAccepted,
                LineItems =
                [
                    new SessionLineItemOptions { Price = config.Value.SubscriptionID, Quantity = 1 }
                ],
                SubscriptionData = new SessionSubscriptionDataOptions()
                {
                    TrialSettings = new SessionSubscriptionDataTrialSettingsOptions()
                    {
                        EndBehavior = new SessionSubscriptionDataTrialSettingsEndBehaviorOptions()
                        {
                            MissingPaymentMethod = "cancel"
                        }
                    },
                    TrialPeriodDays = config.Value.TrialPeriodInDays,
                },
                SuccessUrl = config.Value.PaymentSuccessUrl,
                CancelUrl = config.Value.PaymentCancelUrl,
                ClientReferenceId = Guid.NewGuid().ToString(),
                PaymentMethodCollection = "always",
                ExpiresAt = expirationDate,
            };

            SessionService sessionService = new();
            Checkout.Session? session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

            if (session != null)
            {
                return SubscriptionCheckoutSessionEntity.Create(
                    session.Url,
                    session.Id,
                    session.ClientReferenceId,
                    session.ExpiresAt,
                    config.Value.SubscriptionID
                );
            }

            logger.LogError("Stripe error while initiating subscription payment, stripe session is NULL");
            return Result.Failure<SubscriptionCheckoutSessionEntity>(AppErrors.GenericError);
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while initiating subscription payment");
            return Result.Failure<SubscriptionCheckoutSessionEntity>(AppErrors.GenericError);
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while initiating subscription payment");
            return Result.Failure<SubscriptionCheckoutSessionEntity>(AppErrors.UnknownError);
        }
    }

    public async Task<Result<string>> GetSubscriptionPanelUrlAsync(string externalCustomerID, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting subscription panel URL for user {userId}", externalCustomerID);
            BillingPortal.SessionCreateOptions options = new() { Customer = externalCustomerID };
            BillingPortal.SessionService sessionService = new();
            BillingPortal.Session? session =
                await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

            return session.Url;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error getting the subscription");
            return Result.Failure<string>(AppErrors.GenericError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting the subscription");
            return Result.Failure<string>(AppErrors.UnknownError);
        }
    }

    public async Task<Result<string>> RegisterCustomerAsync(UserProfileEntity payer,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Registering customer against stripe with ID {userId}", payer.Id);
            CustomerService customerService = new();
            Customer customer = await customerService.CreateAsync(
                new CustomerCreateOptions
                {
                    Name = $"{payer.FirstName} {payer.MiddleName} {payer.LastName}",
                    Phone = payer.PhoneNumber,
                    Email = payer.Email,
                    Metadata = new Dictionary<string, string> { { "userId", payer.Id } },
                }, new RequestOptions() { IdempotencyKey = $"create-customer:${payer.Id}" },
                cancellationToken: cancellationToken);

            return customer.Id;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating the customer");
            return Result.Failure<string>(AppErrors.GenericError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating the customer");
            return Result.Failure<string>(AppErrors.UnknownError);
        }
    }

    public async Task<Result<Unit>> CancelSubscription(string externalSubscriptionID,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Cancelling subscription  {subscriptionID}", externalSubscriptionID);
            SubscriptionService service = new();

            await service.UpdateAsync(externalSubscriptionID,
                new SubscriptionUpdateOptions() { CancelAtPeriodEnd = true, }, cancellationToken: cancellationToken);

            return Unit.Value;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating the customer");
            return Result.Failure<Unit>(AppErrors.GenericError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating the customer");
            return Result.Failure<Unit>(AppErrors.UnknownError);
        }
    }

    public async Task<Result<Unit>> ReactivateSubscription(string externalSubscriptionID,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Reactivating subscription {userId}", externalSubscriptionID);
            SubscriptionService service = new();

            await service.UpdateAsync(externalSubscriptionID,
                new SubscriptionUpdateOptions() { CancelAtPeriodEnd = false, }, cancellationToken: cancellationToken);

            return Unit.Value;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating the customer");
            return Result.Failure<Unit>(AppErrors.GenericError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating the customer");
            return Result.Failure<Unit>(AppErrors.UnknownError);
        }
    }

    public async Task<Result<Unit>> CancelCheckoutSession(string sessionID, CancellationToken cancellationToken)
    {
        try
        {
            SessionService service = new();
            await service.ExpireAsync(sessionID, cancellationToken: cancellationToken);
            logger.LogInformation("Subscription payment for session {PaymentSessionID} cancelled successfully",
                sessionID);
            return Unit.Value;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while expiring the checkout session in stripe");
            return Result.Failure<Unit>(AppErrors.GenericError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while expiring the checkout session in stripe");
            return Result.Failure<Unit>(AppErrors.UnknownError);
        }
    }
}
