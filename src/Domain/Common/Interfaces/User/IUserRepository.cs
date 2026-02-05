using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.User;

/// <summary>
/// Defines the contract for user persistence operations in the application.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Asynchronously adds a new user to the repository.
    /// </summary>
    /// <param name="userEntity">The user entity to be added.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is a string
    /// representing the unique identifier of the added user, if successful.
    /// </returns
    Task<Result<string>> AddAsync(Entities.UserEntity userEntity);

    /// <summary>
    /// Asynchronously checks if a user exists in the repository by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to check for existence.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is a boolean indicating
    /// whether a user with the specified email exists in the repository.
    /// </returns>
    Task<Result<bool>> ExistsByEmailAsync(string email);

    /// <summary>
    /// Asynchronously retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is a <see cref="UserEntity"/>
    /// representing the user associated with the specified identifier, if found.
    /// </returns>
    Task<Result<Entities.UserEntity>> GetByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves users from the repository with the specified role.
    /// </summary>
    /// <param name="role">The role of the users to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is an array of <see cref="UserEntity"/> objects
    /// corresponding to the specified role.
    /// </returns>
    Task<Result<IEnumerable<UserEntity>>> GetByRoleAsync(UserRole role);
}
