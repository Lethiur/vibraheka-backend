using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Payments.Ports.Out;

namespace VibraHeka.Domain.Payments.Services;

public class CustomerService(IUserRepository userRepository, IPaymentsPort paymentPort, ILogger<CustomerService> logger)
{
    /// <summary>
    /// Retrieves a customer associated with the specified user ID. If the user does not have a Customer ID,
    /// attempts to register the user as a customer and update the Customer ID in the system.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A Result object containing the UserEntity with updated customer information if successful;
    /// otherwise, a Result object indicating the failure reason.
    /// </returns>
    public async Task<Result<UserEntity>> GetCustomerByUserIDAsync(string userID, CancellationToken token)
    {
        Result<UserEntity> userResult = await userRepository.GetByIdAsync(userID, token);
        if (userResult.IsFailure)
        {
            logger.LogError("Failed to get user by ID: {Error}", userResult.Error);
            return Result.Failure<UserEntity>(userResult.Error);
        }

        UserEntity user = userResult.Value;

        if (!string.IsNullOrEmpty(user.CustomerID))
        {
            return user;
        }

        (bool _, bool isFailure, string? value, string? error) = await paymentPort.RegisterCustomerAsync(ref user, token);

        if (isFailure)
        {
            logger.LogError("Failed to register user with ID {UserID} customer: {Error}", user.Id, error);
            return Result.Failure<UserEntity>(error);
        }

        user.CustomerID = value;

        Result<Unit> updateCustomerIdAsync = await userRepository.UpdateCustomerIDAsync(user.Id, user.CustomerID, token);

        if (!updateCustomerIdAsync.IsFailure)
        {
            return userResult;
        }

        logger.LogError("Failed to update customer ID for user with ID {UserID}: {Error}", user.Id, updateCustomerIdAsync.Error);
        return Result.Failure<UserEntity>(updateCustomerIdAsync.Error);


    }
}
