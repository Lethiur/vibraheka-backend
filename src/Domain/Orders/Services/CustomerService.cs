using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Orders.Ports.Out;

namespace VibraHeka.Domain.Orders.Services;

public class CustomerService(IUserRepository userRepository, IPaymentsPort paymentPort, ILogger<CustomerService> logger)
{
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

        (bool isSuccess, bool isFailure, string? value, string? error) = await paymentPort.RegisterCustomerAsync(user, token);

        if (error != null)
        {
            logger.LogError("Failed to register user with ID {UserID} customer: {Error}", user.Id, error);
            return Result.Failure<UserEntity>(error);
        }
        
        user.CustomerID = value;
        
        
        
        return userResult;
    }
}
