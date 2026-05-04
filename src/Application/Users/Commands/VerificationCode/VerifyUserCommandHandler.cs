using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Models.Results;

namespace VibraHeka.Application.Users.Commands.VerificationCode;

public class VerifyUserCommandHandler(
    IPasswordResetTokenService passwordResetTokenService,
    IUserCodeService userCodeService,
    IUserService userService,
    ILogger<VerifyUserCommandHandler> logger) : IRequestHandler<VerifyUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(VerifyUserCommand request, CancellationToken cancellationToken)
    {
        Result<PasswordResetTokenData> tokenValidationResult = await passwordResetTokenService
            .ValidateAndReadToken(request.EncryptedCode)
            .Ensure(token => EnsureTokenNotUsedAsync(token, cancellationToken), UserErrors.PasswordResetTokenAlreadyUsed)
            .TapError(error => logger.LogWarning(
                "Replay protection check failed. Error: {Error}",
                error))
            .Tap(token =>
                logger.LogInformation("Password reset token validated for email {Email}", token.Email));

        if (tokenValidationResult.IsFailure)
        {
            return Result.Failure<Unit>(tokenValidationResult.Error);
        }

        Result<Unit> confirmationResult = await tokenValidationResult.BindTry(token => userService.ConfirmUserAsync(token.Email, token.CognitoCode));

        if (confirmationResult.IsFailure)
        {
            tokenValidationResult.Tap(token => logger.LogInformation("Cognito password recovery confirmed for email {Email}", token.Email))
                .TapError(error => logger.LogWarning("Cognito password recovery failed with error {Error}", error));
            return confirmationResult;
        }

        Result<Unit> replayMarkerResult = await userCodeService.MarkPasswordResetTokenAsUsedAsync(
                tokenValidationResult.Value.Email,
                tokenValidationResult.Value.TokenId,
                tokenValidationResult.Value.ExpiresAt,
                cancellationToken)
            .Tap(_ => logger.LogInformation("Password reset token replay marker stored"))
            .TapError(error => logger.LogWarning("Could not store replay marker: {Error}", error));

        // Password is already changed in Cognito; replay marker persistence is best-effort.
        return confirmationResult;
    }

    private Task<bool> EnsureTokenNotUsedAsync(
        PasswordResetTokenData token,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating replay protection for token {TokenId}", token.TokenId);

        return userCodeService.IsPasswordResetTokenUsedAsync(
            token.Email,
            token.TokenId,
            cancellationToken).Match(b => !b, e =>
        {
            logger.LogWarning(
                "Replay protection check failed for token {TokenId}. Error: {Error}",
                token.TokenId,
                e);
            return false;
        });
    }
}
