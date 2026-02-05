using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Application.Common.Extensions.Results;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Provides functionality to authorize and validate user privileges
/// within the application.
/// </summary>
public class PrivilegeService(
    IUserRepository UsersRepository,
    IActionLogRepository ActionLogRepository,
    ILogger<IPrivilegeService> Logger) : IPrivilegeService
{
    /// <summary>
    /// Checks if a user has a specific role within the system.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to check.</param>
    /// <param name="role">The role to validate against the user's current role.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>
    /// A <c>Result</c> containing a boolean value, where <c>true</c> indicates the user has the specified role,
    /// and <c>false</c> otherwise. If the user is not found, the result will contain an error.
    /// </returns>
    public async Task<Result<bool>> HasRoleAsync(string userId, UserRole role, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = await UsersRepository.GetByIdAsync(userId, cancellationToken);

        if (user.IsFailure)
        {
            Logger.LogError($"User with ID {userId} not found.");
        }

        ;

        return user.Map(userDB => userDB.Role == role);
    }

    /// <summary>
    /// Determines whether a user has permission to edit a specific resource.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose privileges are being validated.</param>
    /// <param name="resourceId">The unique identifier of the resource to check edit permissions for.</param>
    /// <returns>
    /// A <c>Result</c> containing a boolean value, where <c>true</c> indicates the user has permission to edit the resource,
    /// and <c>false</c> otherwise. If an error occurs during the check, the result will contain the associated error.
    /// </returns>
    public Task<Result<bool>> CanEditResource(string userId, string resourceId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Determines whether a user is permitted to execute a specific action based on recent activity and other contextual rules.
    /// </summary>
    /// <param name="userId">The unique identifier of the user attempting the action.</param>
    /// <param name="action">The type of action the user wishes to execute.</param>
    /// <param name="cancellationToken">The cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>
    /// A <c>Result</c> containing a boolean value, where <c>true</c> indicates the user can execute the specified action,
    /// and <c>false</c> otherwise. If the validation fails due to errors, the result will contain the corresponding error details.
    /// </returns>
    public Task<Result<bool>> CanExecuteAction(string userId, ActionType action, CancellationToken cancellationToken)
    {
        return  Maybe.From(userId).ToResult(PrivilegeErrors.InvalidID)
            .BindTry(id => ActionLogRepository.GetActionLogForUser(id, action, cancellationToken))
            .BindTry(async entity =>
            {
                TimeSpan timeElapsed = DateTimeOffset.UtcNow - entity.Timestamp;
                if (timeElapsed.TotalMinutes < 1)
                {
                    return false;
                }
                entity.Timestamp = DateTimeOffset.UtcNow;
                return await ActionLogRepository.SaveActionLog(entity, cancellationToken).Map(_ => true);
            })
            .OnFailureCompensateWhen(
                error => error == ActionLogErrors.ActionLogNotFound,
                _ =>
            {
                ActionLogEntity entity = new()
                {
                    ID = userId, Action = action, Timestamp = DateTimeOffset.UtcNow
                };
                return ActionLogRepository.SaveActionLog(entity, cancellationToken).Map(_ => true);
            });
    }
}
