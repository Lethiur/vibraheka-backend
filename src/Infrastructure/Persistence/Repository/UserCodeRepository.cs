using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Repository implementation for persisted user code markers in DynamoDB.
/// </summary>
public class UserCodeRepository(
    AWSConfig config,
    IDynamoDBContext context,
    UsersCodeMapper mapper,
    ILogger<GenericDynamoRepository<UserCodeDBModel>> logger)
    : GenericDynamoRepository<UserCodeDBModel>(context, config.UserCodesTable, logger), IUserCodeRepository
{
    /// <summary>
    /// Saves a user code marker.
    /// </summary>
    /// <param name="userCode">Domain entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Persistence result.</returns>
    public Task<Result<Unit>> SaveCode(UserCodeEntity userCode, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(userCode), cancellationToken);
    }

    /// <summary>
    /// Retrieves a user code marker by token identifier.
    /// </summary>
    /// <param name="tokenId">Token identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Domain entity result or mapped failure.</returns>
    public Task<Result<UserCodeEntity>> GetCodeEntityByTokenId(string tokenId, CancellationToken cancellationToken)
    {
        return FindByID(tokenId, cancellationToken).MapError(error =>
        {
            return error switch
            {
                GenericPersistenceErrors.NoRecordsFound => UserCodeErrors.NoRecordFound,
                _ => AppErrors.GenericError
            };
        }).Map(mapper.ToDomain);
    }
}
