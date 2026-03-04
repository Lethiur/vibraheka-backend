using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Provides services for managing payments and subscriptions using Stripe integration.
/// </summary>
public class PaymentService(IPaymentRepository PaymentRepository, IUserRepository UserRepository) : IPaymentService
{
    /// <summary>
    /// Prepares the subscription payment flow and returns all data required by the application layer.
    /// </summary>
    /// <param name="userID">The unique identifier of the user requesting the subscription.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if needed.</param>
    /// <returns>A result containing the prepared subscription context.</returns>
    public Task<Result<SubscriptionContext>> PrepareSubscriptionAsync(string userID,
        CancellationToken cancellationToken)
    {
        return GetUserByID(userID, cancellationToken)
            .BindTry(async user =>
            {
                if (string.IsNullOrEmpty(user.CustomerID))
                {
                    return await PaymentRepository.RegisterCustomerAsync(user, cancellationToken)
                        .BindTry(customerId =>
                        {
                            user.CustomerID = customerId;
                            return UserRepository.AddAsync(user);
                        })
                        .Map(_ => user);
                }

                return user;
            })
            .BindTry(userEntity => PaymentRepository.InitiateSubscriptionPaymentAsync(userEntity, cancellationToken)
                .Map(checkoutSession => new SubscriptionContext
                {
                    UserID = userEntity.Id,
                    ExternalCustomerID = userEntity.CustomerID,
                    CheckoutSession = checkoutSession
                }))
            .MapError(error =>
            {
                return error switch
                {
                    InfrastructureSubscriptionErrors.StripeError => SubscriptionErrors.ErrorWhileSubscribing,
                    GenericPersistenceErrors.GeneralError => SubscriptionErrors.ErrorWhileSubscribing,
                    _ => error
                };
            });
    }

    /// <summary>
    /// Registers a subscription for a specific user, initiating the payment process and associating the subscription details.
    /// </summary>
    /// <param name="userID">The unique identifier of the user for whom the subscription is being registered.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if needed.</param>
    /// <returns>A result containing the subscription ID if successful, or an error message in case of failure.</returns>
    public Task<Result<SubscriptionCheckoutSessionEntity>> RegisterSubscriptionAsync(string userID,
        CancellationToken cancellationToken)
    {
        return PrepareSubscriptionAsync(userID, cancellationToken)
            .Map(preparation => preparation.CheckoutSession);
    }

    /// <summary>
    /// Retrieves the URL for accessing the subscription details page associated with a specific user.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose subscription details URL is to be retrieved.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if required.</param>
    /// <returns>A result containing the subscription details URL if successful, or an error message if the operation fails.</returns>
    public Task<Result<string>> GetSubscriptionDetailsUrlAsync(string userID, CancellationToken cancellationToken)
    {
        return GetUserByID(userID, cancellationToken)
            .BindTry(userEntity => PaymentRepository.GetSubscriptionPanelUrlAsync(userEntity, cancellationToken));
    }

    /// <summary>
    /// Retrieves a user entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">The token used to signal cancellation of the operation.</param>
    /// <returns>A result containing the user entity if found, or an error indicating that the user ID is invalid.</returns>
    public Task<Result<UserEntity>> GetUserByID(string id, CancellationToken cancellationToken)
    {
        return Maybe.From(id)
            .ToResult(UserErrors.InvalidUserID)
            .Ensure(userID => !string.IsNullOrWhiteSpace(userID), UserErrors.InvalidUserID)
            .BindTry(userID => UserRepository.GetByIdAsync(userID, cancellationToken))
            .MapError(error =>
            {
                return error switch
                {
                    InfrastructureUserErrors.UserNotFound => UserErrors.UserNotFound,
                    _ => error
                };
            });
    }

    /// <summary>
    /// Cancels an existing subscription payment tied to the provided subscription checkout session entity.
    /// </summary>
    /// <param name="entity">The subscription checkout session entity containing details of the subscription to be canceled.</param>
    /// <param name="token">The cancellation token to allow the operation to be canceled if necessary.</param>
    /// <returns>A result indicating success or failure of the cancellation operation.</returns>
    public Task<Result<Unit>> CancelSubscriptionPayment(SubscriptionCheckoutSessionEntity entity,
        CancellationToken token)
    {
        return Maybe.From(entity)
            .ToResult(SubscriptionErrors.ErrorWhileSubscribing)
            .BindTry(toInsert => PaymentRepository.CancelSubscriptionPayment(toInsert, token));
    }
}
