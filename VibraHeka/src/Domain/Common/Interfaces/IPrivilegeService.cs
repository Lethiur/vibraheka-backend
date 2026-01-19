using CSharpFunctionalExtensions;
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
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object wrapping a boolean value indicating whether the user has the specified role.</returns>
    public Task<Result<bool>> HasRoleAsync(string userId, UserRole role);

    /// <summary>
    /// Determines whether a user has permission to edit a specific resource.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose privileges are being validated.</param>
    /// <param name="resourceId">The unique identifier of the resource to check edit permissions for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a Result object wrapping a boolean value indicating whether the user has permission to edit the specified resource.
    /// </returns>
    public Task<Result<bool>> CanEditResource(string userId, string resourceId);
}
    
