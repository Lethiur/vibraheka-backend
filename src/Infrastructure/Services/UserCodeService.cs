using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using MediatR;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;

namespace VibraHeka.Infrastructure.Services;

/// <summary>
/// Service responsible for replay-protection token marker operations.
/// </summary>
public class UserCodeService(
    IUserCodeRepository userCodeRepository,
    ILogger<UserCodeService> logger) : IUserCodeService
{
    /// <summary>
    /// Checks if a password reset token has already been used.
    /// </summary>
    /// <param name="email">Email expected in the consumed token record.</param>
    /// <param name="tokenId">Token identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when a matching consumed marker exists.</returns>
    public Task<Result<bool>> IsPasswordResetTokenUsedAsync(string email, string tokenId, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Checking replay marker for password reset token. Email: {Email}, TokenId: {TokenId}",
            email,
            tokenId);

        return Result.Success(tokenId)
            .BindTry(id => userCodeRepository.GetCodeEntityByTokenId(id, cancellationToken))
            .Map(_ => true)
            .OnFailureCompensate(error =>
                error == UserCodeErrors.NoRecordFound
                    ? Result.Success(false)
                    : Result.Failure<bool>(error))
            .Tap(isUsed => logger.LogInformation(
                "Replay marker check finished for token {TokenId}. Used: {IsUsed}",
                tokenId,
                isUsed))
            .TapError(error => logger.LogWarning(
                "Replay marker check failed for token {TokenId}. Error: {Error}",
                tokenId,
                error));
    }

    /// <summary>
    /// Stores the consumed-token marker used to prevent replay.
    /// </summary>
    /// <param name="email">Email associated with token owner.</param>
    /// <param name="tokenId">Token identifier.</param>
    /// <param name="expiresAt">Token expiration timestamp used for DynamoDB TTL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Persistence result.</returns>
    public Task<Result<Unit>> MarkPasswordResetTokenAsUsedAsync(
        string email,
        string tokenId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Storing replay marker for password reset token. Email: {Email}, TokenId: {TokenId}",
            email,
            tokenId);

        UserCodeEntity usedToken = new()
        {
            UserEmail = email,
            ActionType = ActionType.PasswordReset,
            Code = tokenId,
            ExpiresAtUnix = expiresAt.ToUnixTimeSeconds(),
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            CreatedBy = email,
            LastModifiedBy = email,
        };

        return Result.Success(usedToken)
            .BindTry(token => userCodeRepository.SaveCode(token, cancellationToken))
            .Tap(_ => logger.LogInformation(
                "Replay marker stored for token {TokenId}",
                tokenId))
            .TapError(error => logger.LogWarning(
                "Failed to store replay marker for token {TokenId}. Error: {Error}",
                tokenId,
                error));
    }
}
