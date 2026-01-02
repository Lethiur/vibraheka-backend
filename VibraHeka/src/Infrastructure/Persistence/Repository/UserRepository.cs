using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Represents a repository for managing user persistence operations utilizing Amazon DynamoDB.
/// </summary>
public class UserRepository(IDynamoDBContext context, IConfiguration config) : IUserRepository
{
    /// <summary>
    /// Adds a new user to the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="user">The user entity to be added to the DynamoDB users table.</param>
    /// <returns>A result containing the user's ID if the operation is successful, or an error otherwise.</returns>
    public async Task<Result<string>> AddAsync(User user)
    {
        SaveConfig saveConfig = new()
        {
            OverrideTableName = config["Dynamo:UsersTable"],
        };
        
        await context.SaveAsync(UserDBModel.FromDomain(user), saveConfig);
        return user.Id;
    }

    /// <summary>
    /// Checks if a user exists in the DynamoDB users table by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user to search for in the DynamoDB users table.</param>
    /// <returns>A result containing a boolean value indicating whether the user exists or an error if the operation fails.</returns>
    public async Task<Result<bool>> ExistsByEmailAsync(string email)
    {
        QueryConfig queryConfig = new()
        {
            IndexName = "EmailIndex",
            OverrideTableName = config["Dynamo:UsersTable"]
        };
        
        List<User>? results = await context.QueryAsync<User>(email, queryConfig).GetRemainingAsync();
        return results?.Count > 0;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier from the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be retrieved.</param>
    /// <returns>A result containing the user entity if the operation is successful, or an error otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown if the method is not implemented.</exception>
    public async Task<Result<User>> GetByIdAsync(string id)
    {
        LoadConfig configuration = new()
        {
            OverrideTableName = config["Dynamo:UsersTable"],
        };

        try
        {
            UserDBModel? model = await context.LoadAsync<UserDBModel>(id, configuration);
            return model != null ? Result.Success(model.ToDomain()) : Result.Failure<User>(InfrastructureUserErrors.UserNotFound);
        }
        catch (Exception e)
        {
            return Result.Failure<User>(e.Message);
        }
    }
}
