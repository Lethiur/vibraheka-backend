using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Exceptions;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class UserCodeRepository(
    AWSConfig config,
    IDynamoDBContext context,
    UsersCodeMapper mapper,
    ILogger<GenericDynamoRepository<UserCodeDBModel>> logger)
    : GenericDynamoRepository<UserCodeDBModel>(context, config.UserCodesTable, logger), IUserCodeRepository
{
    public Task<Result<Unit>> SaveCode(UserCodeEntity userCode, CancellationToken cancellationToken)
    {
        return Save(mapper.FromDomain(userCode), cancellationToken);
    }

    public Task<Result<UserCodeEntity>> GetCodeEntityFromEmail(string email, CancellationToken cancellationToken)
    {
        return FindByID(email, cancellationToken).MapError(error =>
        {
            return error switch
            {
                GenericPersistenceErrors.NoRecordsFound => UserCodeErrors.NoRecordFound,
                _ => UserCodeErrors.NoRecordFound
            };
        }).Map(mapper.ToDomain);
    }
}
