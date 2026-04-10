using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.Output;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.AWS.DynamoDB.Users.Adapters;

public class PrivilegeAdapter(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    IOptionsMonitor<AWSConfig> config,
    ILogger<PrivilegeAdapter> logger) :
    GenericDynamoRepository<UserProfileEntity>(context, client, config.CurrentValue.UsersTable, logger),
    UserPrivilegePort
{
    public Task<Result<bool>> HasRoleAsync(string userId, UserRole role, CancellationToken cancellationToken)
    {
        return FindByID(userId, cancellationToken).Map(user => user.Role == role);
    }
}
