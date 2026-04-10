using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.AWS.DynamoDB.Errors;
using Infrastructure.AWS.DynamoDB.Users.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.Users.Adapters;

/// <summary>
/// Represents a repository for managing user persistence operations using Amazon DynamoDB.
/// </summary>
public class UserProfileAdapter(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    IOptionsMonitor<AWSConfig> config,
    ILogger<UserProfileAdapter> logger,
    UserProfileMapper mapper) :
    GenericDynamoRepository<UserProfileDBModel>(context, client, config.CurrentValue.UsersTable, logger),
    UserProfilePort
{
    /// <summary>
    /// Adds a new user to the DynamoDB users table asynchronously.
    /// </summary>
    /// <param name="userProfileEntity">The user entity to be added to the DynamoDB users table.</param>
    /// <param name="cancellationToken">Token used to cancel the task before completion</param>
    /// <returns>A result containing the user's ID if the operation is successful, or an error otherwise.</returns>
    public async Task<Result<string>> SaveAsync(UserProfileEntity userProfileEntity,
        CancellationToken cancellationToken)
    {
        Result<Unit> saveResult = await Save(mapper.FromDomain(userProfileEntity), cancellationToken);
        return saveResult.Map(_ => userProfileEntity.Id);
    }

    /// <summary>
    /// Checks if a user exists in the DynamoDB users table by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user to search for in the DynamoDB users table.</param>
    /// <returns>A result containing a boolean value indicating whether the user exists or an error if the operation fails.</returns>
    public async Task<Result<bool>> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        Result<UserProfileDBModel> findOneByIndex = await FindOneByIndex("EmailIndex", email, cancellationToken);

        return findOneByIndex.MapError(error =>
        {
            return error switch
            {
                GenericPersistenceErrors.NoRecordsFound => UserErrors.UserNotFound,
                _ => UserErrors.UnexpectedError
            };
        }).Map(_ => true);
    }

    public Task<Result<UserProfileEntity>> GetProfileByUserId(string userID, CancellationToken cancellationToken)
    {
        return FindByID(userID, cancellationToken).Map(mapper.ToDomain);
    }

    public Task<Result<Unit>> UpdateUserProfile(UserProfileEntity newUserProfileData, string updater,
        CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(newUserProfileData), cancellationToken);
    }
    
    /// <summary>
    /// Asynchronously retrieves users from the repository with the specified role.
    /// </summary>
    /// <param name="role">The role of the users to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is an array of <see cref="UserProfileEntity"/> objects
    /// corresponding to the specified role.
    /// </returns>
    public Task<Result<IEnumerable<UserProfileEntity>>> GetByRoleAsync(UserRole role, CancellationToken token)
    {
        return FindByIndex("Role-Index", role, token).Map(enumerable => enumerable.Select(mapper.ToDomain));
    }
}
