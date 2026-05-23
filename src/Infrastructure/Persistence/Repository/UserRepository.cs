using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Represents a repository for managing user persistence operations utilizing Amazon DynamoDB.
/// </summary>
public class UserRepository(IDynamoDBContext context, IAmazonDynamoDB client, ILogger<UserRepository> logger)
    : GenericDynamoRepository<UserDBModel>(context, client, logger), IUserRepository
{
    /// <summary>
    /// Adds a new user to the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="userEntity">The user entity to be added to the DynamoDB users table.</param>
    /// <returns>A result containing the user's ID if the operation is successful, or an error otherwise.</returns>
    public async Task<Result<string>> AddAsync(UserEntity userEntity)
    {
        Result<Unit> result = await Save(UserDBModel.FromDomain(userEntity), CancellationToken.None);
        return result.Map(_ => userEntity.Id);
    }

    /// <summary>
    /// Checks if a user exists in the DynamoDB users table by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user to search for in the DynamoDB users table.</param>
    /// <returns>A result containing a boolean value indicating whether the user exists or an error if the operation fails.</returns>
    public async Task<Result<bool>> ExistsByEmailAsync(string email)
    {
        Result<UserDBModel> findOneByIndex = await FindOneByIndex("EmailIndex", email, CancellationToken.None);
        if (findOneByIndex is { IsFailure: true, Error: GenericPersistenceErrors.NoRecordsFound })
        {
            return false;
        }
        return findOneByIndex.Map(_ => true);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier from the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be retrieved.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>A result containing the user entity if the operation is successful, or an error otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown if the method is not implemented.</exception>
    public Task<Result<UserEntity>> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        return FindByID(id, cancellationToken).Map(m => m.ToDomain())
            .MapError(e =>
            {
                return e switch
                {
                    _ => UserErrors.UserNotFound
                };
            });
    }

    /// <summary>
    /// Retrieves all user entities from the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing a collection of user entities if the operation is successful, or an error otherwise.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public Task<Result<IEnumerable<UserEntity>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return GetAll(cancellationToken).Map(models => models.Select(m => m.ToDomain()));
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
    public Task<Result<List<UserEntity>>> GetByRoleAsync(UserRole role)
    {
        return FindAllByIndexAsync("Role-Index", role.ToString(), CancellationToken.None)
            .Map(models => models.Select(m => m.ToDomain()).ToList())
            .MapError(e => UserErrors.UserNotFound);
    }

    /// <summary>
    /// Updates the customer ID for a specific user in the database asynchronously.
    /// </summary>
    /// <param name="customerId">The new customer ID to be assigned to the user.</param>
    /// <param name="userId">The ID of the user whose customer ID is to be updated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or failure of the operation.</returns>
    public Task<Result<Unit>> UpdateCustomerIDAsync(string customerId, string userId,
        CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> key = new()
        {
            { nameof(UserEntity.Id), new AttributeValue { S = userId } }
        };

        DynamoExpression update = new()
        {
            Expression = "set #status = :status",
            AttributeNames = { ["#status"] = "CustomerID" },
            AttributeValues = { { ":status", new AttributeValue { S = customerId } } }
        };

        return UpdateAsync(key, update, null, cancellationToken);
    }
}
