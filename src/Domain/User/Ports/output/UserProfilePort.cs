using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.User.Ports.output;

public interface UserProfilePort
{
    /// <summary>
    /// Retrieves a user's information based on the provided unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">The token used to cancel the task before it completes</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="User"/> object with the details of the user if found; otherwise, null.
    /// </returns>
    Task<Result<UserProfileEntity>> GetProfileByUserId(string userID, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates the user's profile information in the system.
    /// </summary>
    /// <param name="newUserProfileData">An instance of <see cref="UserProfileEntity"/> containing the updated user data.</param>
    /// <param name="updater">The identifier of the person or system performing the update operation.</param>
    /// <param name="cancellationToken">The token used to cancel the task</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> indicating whether the update operation was successful.
    /// </returns>
    Task<Result<Unit>> UpdateUserProfile(UserProfileEntity newUserProfileData, string updater, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously adds a new user to the repository.
    /// </summary>
    /// <param name="userProfileEntity">The user entity to be added.</param>
    /// <param name="cancellationToken">The token used to cancel the operation</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result"/> where T is a string
    /// representing the unique identifier of the added user, if successful.
    /// </returns
    Task<Result<string>> SaveAsync(UserProfileEntity userProfileEntity, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously checks if a user exists in the repository by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to check for existence.</param>
    /// <param name="cancellationToken">The token used to cancel the task beforehand</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is a boolean indicating
    /// whether a user with the specified email exists in the repository.
    /// </returns>
    Task<Result<bool>> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves users from the repository with the specified role.
    /// </summary>
    /// <param name="role">The role of the users to retrieve.</param>
    /// <param name="cancellationToken">The token used to cancel the task beforehand</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> where T is an array of <see cref="UserProfileEntity"/> objects
    /// corresponding to the specified role.
    /// </returns>
    Task<Result<IEnumerable<UserProfileEntity>>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken);
}
