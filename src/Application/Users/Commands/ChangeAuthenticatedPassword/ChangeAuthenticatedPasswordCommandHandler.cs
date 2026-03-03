using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.Users.Commands.ChangeAuthenticatedPassword;

/// <summary>
/// Handles authenticated user password changes.
/// </summary>
public class ChangeAuthenticatedPasswordCommandHandler(
    ICurrentUserService currentUserService,
    IUserService userService,
    ILogger<ChangeAuthenticatedPasswordCommandHandler> logger)
    : IRequestHandler<ChangeAuthenticatedPasswordCommand, Result<Unit>>
{
    /// <summary>
    /// Changes the current authenticated user password in Cognito.
    /// </summary>
    /// <param name="request">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    public Task<Result<Unit>> Handle(ChangeAuthenticatedPasswordCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing password for authenticated user {UserId}", currentUserService.UserId);

        return Result.Success(currentUserService.AccessToken)
            .Ensure(token => !string.IsNullOrWhiteSpace(token), UserErrors.NotAuthorized)
            .BindTry(accessToken => userService.ChangePasswordAsync(
                accessToken!,
                request.CurrentPassword,
                request.NewPassword,
                cancellationToken))
            .Tap(_ => logger.LogInformation(
                "Authenticated password change completed for user {UserId}",
                currentUserService.UserId))
            .TapError(error => logger.LogWarning(
                "Authenticated password change failed for user {UserId} with error {Error}",
                currentUserService.UserId,
                error));
    }
}
