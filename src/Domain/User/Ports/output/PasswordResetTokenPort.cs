using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.User.Ports.output;

public interface PasswordResetTokenPort
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
    /// Persists a user code entity.
    /// </summary>
    /// <param name="userCode">Entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation result.</returns>
    Task<Result<Unit>> SaveCode(UserCodeEntity userCode, CancellationToken cancellationToken);
    

    /// <summary>
    /// Retrieves a user code entity by token identifier.
    /// </summary>
    /// <param name="tokenId">Token identifier stored as code primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity result or failure when not found.</returns>
    Task<Result<UserCodeEntity>> GetCodeEntityByTokenId(string tokenId, CancellationToken cancellationToken);
}
