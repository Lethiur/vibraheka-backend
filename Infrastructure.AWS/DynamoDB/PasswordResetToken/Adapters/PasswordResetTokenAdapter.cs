using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.AWS.DynamoDB.Errors;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;
using UsersCodeMapper = Infrastructure.AWS.DynamoDB.PasswordResetToken.Mappers.UsersCodeMapper;

namespace Infrastructure.AWS.DynamoDB.PasswordResetToken.Adapters;

/// <summary>
/// Repository implementation for persisted user code markers in DynamoDB.
/// </summary>
public class PasswordResetTokenAdapter(
    IOptionsMonitor<AWSConfig> config,
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    UsersCodeMapper mapper,
    ILogger<GenericDynamoRepository<UserCodeDBModel>> logger)
    : GenericDynamoRepository<UserCodeDBModel>(context, client, config.CurrentValue.UserCodesTable, logger), PasswordResetTokenPort
{
    public Task<Result<bool>> IsPasswordResetTokenUsedAsync(string email, string tokenId,
        CancellationToken cancellationToken)
    {
        return FindByID(tokenId, cancellationToken).Map(_ => false).OnFailureCompensate(error =>
            error == GenericPersistenceErrors.NoRecordsFound ? Result.Success(true) : Result.Failure<bool>(error));
    }

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
                _ => GenericPersistenceErrors.GeneralError
            };
        }).Map(mapper.ToDomain);
    }
}
