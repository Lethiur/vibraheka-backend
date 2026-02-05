using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Represents a repository for managing user persistence operations utilizing Amazon DynamoDB.
/// </summary>
public class UserRepository(IDynamoDBContext context, AWSConfig config) : IUserRepository
{
    /// <summary>
    /// Adds a new user to the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="userEntity">The user entity to be added to the DynamoDB users table.</param>
    /// <returns>A result containing the user's ID if the operation is successful, or an error otherwise.</returns>
    public async Task<Result<string>> AddAsync(UserEntity userEntity)
    {
        SaveConfig saveConfig = new()
        {
            OverrideTableName = config.UsersTable,
        };
        
        await context.SaveAsync(UserDBModel.FromDomain(userEntity), saveConfig);
        return userEntity.Id;
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
            OverrideTableName = config.UsersTable
        };
        
        List<UserDBModel>? results = await context.QueryAsync<UserDBModel>(email, queryConfig).GetRemainingAsync();
        return results?.Count > 0;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier from the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be retrieved.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>A result containing the user entity if the operation is successful, or an error otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown if the method is not implemented.</exception>
    public async Task<Result<UserEntity>> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        LoadConfig configuration = new()
        {
            OverrideTableName = config.UsersTable,
        };

        try
        {
            UserDBModel? model = await context.LoadAsync<UserDBModel>(id, configuration, cancellationToken);
            return model != null ? Result.Success(model.ToDomain()) : Result.Failure<UserEntity>(InfrastructureUserErrors.UserNotFound);
        }
        catch (Exception e)
        {
            return Result.Failure<UserEntity>(e.Message);
        }
    }

    /// <summary>
    /// Asynchronously retrieves users from the repository with the specified role.
    /// </summary>
    /// <param name="role">The role of the users to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is an array of <see cref="UserEntity"/> objects
    /// corresponding to the specified role.
    /// </returns>
    public async Task<Result<IEnumerable<UserEntity>>> GetByRoleAsync(UserRole role)
    {
        QueryConfig queryConfig = new()
        {
            IndexName = "Role-Index",
            OverrideTableName = config.UsersTable
        };

        try
        {
            IAsyncSearch<UserDBModel>? search = context.QueryAsync<UserDBModel>(role, queryConfig);
            List<UserDBModel>? models = await search.GetRemainingAsync();

            if (models == null || models.Count == 0)
            {
                return Result.Success(Enumerable.Empty<UserEntity>());
            }

            return Result.Success(models.Select(m => m.ToDomain()));
        }
        catch (Exception e)
        {
            return Result.Failure<IEnumerable<UserEntity>>($"Error querying users by role: {e.Message}");
        }
    }
}
