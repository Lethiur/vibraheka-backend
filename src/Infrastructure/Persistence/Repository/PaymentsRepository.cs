using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Provides the implementation for managing payment-related operations, such as initiating subscription payments,
/// retrieving subscription panel URLs, and querying order statuses for a user.
/// </summary>
public class PaymentsRepository(StripeConfig Config, ILogger<PaymentsRepository> logger) : IPaymentRepository
{
    /// <summary>
    /// Initiates a payment process for a user's subscription.
    /// </summary>
    /// <param name="payer">
    ///     The user initiating the payment. Contains user-related details such as ID, email, and other relevant information.
    /// </param>
    /// <param name="orderEntity">
    ///     The subscription order entity for which the payment is being initiated. Includes subscription details,
    ///     such as subscription ID, start and end dates, and external identifiers.
    /// </param>
    /// <param name="cancellationToken">Token used to halt the operation mid way if needed</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing a result object with either
    /// the payment initiation URL as a string on success or an error message on failure.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method has not been implemented.
    /// </exception>
    public async Task<Result<string>> InitiateSubscriptionPaymentAsync(UserEntity payer, SubscriptionEntity orderEntity, CancellationToken cancellationToken)
    {
        try
        {
            SessionCreateOptions options = new()
            {
                Mode = "subscription",
                Customer = payer.CustomerID,
                PaymentMethodTypes = Config.PaymentMethodsAccepted,
                LineItems =
                [
                    new SessionLineItemOptions { Price = orderEntity.ExternalSubscriptionItemID, Quantity = 1 }
                ],
                SuccessUrl = Config.PaymentSuccessUrl,
                CancelUrl = Config.PaymentCancelUrl,
                ClientReferenceId = Guid.NewGuid().ToString()
            };

            SessionService sessionService = new();
            Session? session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

            return (session.Url);
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while initiating subscription payment");
            return Result.Failure<string>(InfrastructureSubscriptionErrors.StripeError);
        }
        
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while initiating subscription payment");
            return Result.Failure<string>(GenericPersistenceErrors.GeneralError);
        }
        
    }

    /// <summary>
    /// Retrieves the subscription panel URL for a user to manage their subscription details.
    /// </summary>
    /// <param name="payer">
    ///     The user for whom the subscription panel URL is being generated. Contains user data such as ID and email.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A task that represents the asynchronous operation. The result contains the subscription panel URL as a string on success
    /// or an error message on failure.
    /// </returns>
    /// <exception cref="StripeException">
    /// Thrown if there is an error while communicating with the Stripe API.
    /// </exception>
    public async Task<Result<string>> GetSubscriptionPanelUrlAsync(UserEntity payer,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting subscription panel URL for user {userId}", payer.Id);
            Stripe.BillingPortal.SessionCreateOptions options = new() { Customer = payer.CustomerID };
            Stripe.BillingPortal.SessionService sessionService = new();
            Stripe.BillingPortal.Session? session =
                await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

            return session.Url;
        }
        catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error getting the subscription");
            return Result.Failure<string>(InfrastructureSubscriptionErrors.StripeError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting the subscription");
            return Result.Failure<string>(GenericPersistenceErrors.GeneralError);
        }
    }

    /// <summary>
    /// Registers a new customer within the payment system.
    /// </summary>
    /// <param name="payer">
    /// The user entity containing details such as ID, email, and other personal information
    /// required for customer registration.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the registration operation if necessary.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing a result object with
    /// the customer registration ID as a string on success or an error message on failure.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method has not been implemented.
    /// </exception>
    public async Task<Result<string>> RegisterCustomerAsync(UserEntity payer, CancellationToken cancellationToken)
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
                    Metadata = new Dictionary<string, string> { { "userId", payer.Id } }
                }, new RequestOptions()
                {
                    IdempotencyKey  = $"create-customer:${payer.Id}"
                }, cancellationToken: cancellationToken);

            return customer.Id;    
        }  catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while creating the customer");
            return Result.Failure<string>(InfrastructureSubscriptionErrors.StripeError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating the customer");
            return Result.Failure<string>(GenericPersistenceErrors.GeneralError);
        }
        
        
    }

    /// <summary>
    /// Cancels an active subscription for a specific user, setting it to terminate at the end of the current billing period.
    /// </summary>
    /// <param name="subscription">
    /// The subscription entity to be canceled. Includes details such as the external subscription ID necessary for identifying the subscription.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the operation if necessary.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. Contains a result indicating success or failure of the cancellation process.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the user or subscription parameter is null.
    /// </exception>
    /// <exception cref="StripeException">
    /// Thrown if an error occurs while communicating with the Stripe payment gateway.
    /// </exception>
    public async Task<Result<Unit>> CancelSubscriptionForUser( SubscriptionEntity subscription,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Cancelling subscription for user {userId}", subscription.UserID);
            SubscriptionService service = new();
        
             await service.UpdateAsync(subscription.ExternalSubscriptionID, new SubscriptionUpdateOptions()
            {
                CancelAtPeriodEnd = true
            }, cancellationToken: cancellationToken);
            
            return Unit.Value;    
        }  catch (StripeException stripeEx)
        {
            logger.LogError(stripeEx, "Stripe error while cancelling the subscription");
            return Result.Failure<Unit>(InfrastructureSubscriptionErrors.StripeError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while cancelling the subscription");
            return Result.Failure<Unit>(GenericPersistenceErrors.GeneralError);
        }
    }
}
