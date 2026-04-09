using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.User.Ports.output;

public interface UserPrivilegePort
{
    /// <summary>
    /// Verifies if a user has a specific role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="role">The role to check against the user's privileges.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a Result object wrapping a boolean value indicating whether the user has the specified role.</returns>
    public Task<Result<bool>> HasRoleAsync(string userId, UserRole role, CancellationToken cancellationToken);

}
