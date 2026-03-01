using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.Users.Commands.ConfirmPasswordRecovery;

/// <summary>
/// Handles password recovery confirmation with replay protection.
/// </summary>
public class ConfirmPasswordRecoveryCommandHandler(
    IPasswordResetTokenService passwordResetTokenService,
    IUserCodeService userCodeService,
    IUserService userService,
    ILogger<ConfirmPasswordRecoveryCommandHandler> logger)
    : IRequestHandler<ConfirmPasswordRecoveryCommand, Result<Unit>>
{
    /// <summary>
    /// Executes the password recovery confirmation flow.
    /// </summary>
    /// <param name="request">Command payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    public Task<Result<Unit>> Handle(ConfirmPasswordRecoveryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Confirm password recovery command started");

        return Result.Success()
            .BindTry(async () =>
            {
                Result<PasswordResetTokenData> tokenResult = await passwordResetTokenService
                    .ValidateAndReadToken(request.EncryptedToken)
                    .Tap(token => logger.LogInformation("Password reset token validated for email {Email}", token.Email))
                    .BindTry(token => EnsureTokenNotUsedAsync(token, cancellationToken))
                    .TapError(error => logger.LogWarning(
                        "Password recovery token validation failed with error {Error}", error))
                    .BindTry(token => ConfirmPasswordInCognitoAsync(token, request.NewPassword, cancellationToken))
                    .Tap(token => logger.LogInformation("Cognito password recovery confirmed for email {Email}", token.Email))
                    .TapError(error => logger.LogWarning("Cognito password recovery failed with error {Error}", error));

                if (tokenResult.IsFailure)
                {
                    return Result.Failure<Unit>(tokenResult.Error);
                }

                Result<Unit> replayMarkerResult = await SaveUsedTokenAsync(tokenResult.Value, cancellationToken)
                    .Tap(_ => logger.LogInformation("Password reset token replay marker stored"))
                    .TapError(error => logger.LogWarning("Could not store replay marker: {Error}", error));

                // Password is already changed in Cognito; replay marker persistence is best-effort.
                return replayMarkerResult.OnFailureCompensate(_ => Result.Success(Unit.Value));
            });
    }

    private async Task<Result<PasswordResetTokenData>> EnsureTokenNotUsedAsync(
        PasswordResetTokenData token,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating replay protection for token {TokenId}", token.TokenId);
        return await Result.Success(token)
            .BindTry(async currentToken =>
                (await userCodeService.IsPasswordResetTokenUsedAsync(
                        currentToken.Email,
                        currentToken.TokenId,
                        cancellationToken))
                    .Map(isUsed => (Token: currentToken, IsUsed: isUsed)))
            .Ensure(result => !result.IsUsed, UserErrors.PasswordResetTokenAlreadyUsed)
            .Map(result => result.Token)
            .Tap(_ => logger.LogInformation("Replay protection validated for token {TokenId}", token.TokenId))
            .TapError(error => logger.LogWarning(
                "Replay protection check failed for token {TokenId}. Error: {Error}",
                token.TokenId,
                error));
    }

    /// <summary>
    /// Confirms password reset in Cognito and keeps the current token payload for downstream steps.
    /// </summary>
    /// <param name="token">Validated token payload.</param>
    /// <param name="newPassword">New password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token payload when Cognito confirmation succeeds.</returns>
    private async Task<Result<PasswordResetTokenData>> ConfirmPasswordInCognitoAsync(
        PasswordResetTokenData token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        return await userService
            .ConfirmPasswordRecoveryAsync(token.Email, token.CognitoCode, newPassword, cancellationToken)
            .Map(_ => token);
    }

    /// <summary>
    /// Persists the replay marker for a consumed token.
    /// </summary>
    /// <param name="token">Validated token payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Persistence result.</returns>
    private Task<Result<Unit>> SaveUsedTokenAsync(PasswordResetTokenData token, CancellationToken cancellationToken)
    {
        return userCodeService.MarkPasswordResetTokenAsUsedAsync(
            token.Email,
            token.TokenId,
            token.ExpiresAt,
            cancellationToken);
    }
}
