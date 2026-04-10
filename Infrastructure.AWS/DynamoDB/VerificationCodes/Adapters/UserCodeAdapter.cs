#if DEBUG
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Infrastructure.AWS.DynamoDB.VerificationCodes.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.Out;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace Infrastructure.AWS.DynamoDB.VerificationCodes.Adapters;

/// <summary>
/// Provides methods to interact with DynamoDB for managing verification codes associated with users.
/// </summary>
public class UserCodeAdapter(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    IOptionsMonitor<AWSConfig> config,
    VerificationCodeEntityMapper mapper,
    ILogger<UserCodeAdapter> logger) :
    GenericDynamoRepository<VerificationCodeDBModel>(context, client, config.CurrentValue.CodesTable, logger),
    UserCodePort
{
    public Task<Result<VerificationCodeEntity>> GetCodeFor(string email, CancellationToken cancellationToken)
    {
        return FindByID(email, cancellationToken).Map(mapper.ToDomain);
    }
}
#endif
