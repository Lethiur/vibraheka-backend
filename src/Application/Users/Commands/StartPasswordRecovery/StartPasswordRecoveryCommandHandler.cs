using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.Users.Commands.StartPasswordRecovery;

/// <summary>
/// Handles password recovery initialization requests.
/// </summary>
public class StartPasswordRecoveryCommandHandler(
    IUserService userService,
    ILogger<StartPasswordRecoveryCommandHandler> logger)
    : IRequestHandler<StartPasswordRecoveryCommand, Result<Unit>>
{
    /// <summary>
    /// Starts Cognito forgot-password flow for the given email.
    /// </summary>
    /// <param name="request">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result with anti-enumeration compensation.</returns>
    public Task<Result<Unit>> Handle(StartPasswordRecoveryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting password recovery command for email {Email}", request.Email);
        return Result.Success(request.Email)
            .BindTry(email => userService.StartPasswordRecoveryAsync(email))
            .OnFailureCompensate(error =>
                error == UserErrors.UserNotFound
                    ? Result.Success(Unit.Value)
                    : Result.Failure<Unit>(error))
            .Tap(_ => logger.LogInformation("Password recovery command completed for email {Email}", request.Email))
            .TapError(error => logger.LogWarning(
                "Password recovery command failed for email {Email} with error {Error}", request.Email, error));
    }
}
