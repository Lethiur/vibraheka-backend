using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Domain.Common.Interfaces.User;

/// <summary>
/// Provides high-level operations for user code domain flows.
/// </summary>
public interface IUserCodeService
{
    /// <summary>
    /// Checks if a password reset token has already been consumed.
    /// </summary>
    /// <param name="email">Email expected in the consumed token record.</param>
    /// <param name="tokenId">Unique token identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when the token is already marked as used.</returns>
    Task<Result<bool>> IsPasswordResetTokenUsedAsync(string email, string tokenId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a password reset token as used to prevent replay.
    /// </summary>
    /// <param name="email">Email associated to the token.</param>
    /// <param name="tokenId">Unique token identifier.</param>
    /// <param name="expiresAt">Expiration date used to configure persistence TTL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<Result<Unit>> MarkPasswordResetTokenAsUsedAsync(
        string email,
        string tokenId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);
}
