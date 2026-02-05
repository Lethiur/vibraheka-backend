using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces;

/// <summary>
/// Defines a service responsible for managing and verifying user privileges within the system.
/// </summary>
public interface IPrivilegeService
{
    /// <summary>
    /// Verifies if a user has a specific role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="role">The role to check against the user's privileges.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object wrapping a boolean value indicating whether the user has the specified role.</returns>
    public Task<Result<bool>> HasRoleAsync(string userId, UserRole role, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a user has permission to edit a specific resource.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose privileges are being validated.</param>
    /// <param name="resourceId">The unique identifier of the resource to check edit permissions for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result object wrapping a boolean value indicating whether the user has permission to edit the specified resource.
    /// </returns>
    public Task<Result<bool>> CanEditResource(string userId, string resourceId);

    /// <summary>
    /// Determines whether a user has the privilege to execute a specific action.
    /// </summary>
    /// <param name="userId">The unique identifier of the user attempting to perform the specified action.</param>
    /// <param name="action">The type of action to be evaluated against the user's privileges.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result object wrapping a boolean value indicating whether the user is allowed to execute the specified action.
    /// </returns>
    public Task<Result<bool>> CanExecuteAction(string userId, ActionType action, CancellationToken cancellationToken);
}
    
