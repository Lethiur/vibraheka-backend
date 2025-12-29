using CSharpFunctionalExtensions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for user persistence operations in the application.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Asynchronously adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user entity to be added.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is a string
    /// representing the unique identifier of the added user, if successful.
    /// </returns
    Task<Result<string>> AddAsync(User user);

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
}
