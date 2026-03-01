using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces.User;

/// <summary>
/// Defines persistence operations for user code entities.
/// </summary>
public interface IUserCodeRepository
{
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
