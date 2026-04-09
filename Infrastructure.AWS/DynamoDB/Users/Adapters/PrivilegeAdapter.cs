using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.User.Ports.output;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.Repository;

namespace Infrastructure.AWS.DynamoDB.Users.Adapters;

public class PrivilegeAdapter(IDynamoDBContext context, IOptions<AWSConfig> config, ILogger<PrivilegeAdapter> logger) : 
    GenericDynamoRepository<UserProfileEntity>(context, config.Value.UsersTable, logger),
    UserPrivilegePort
{
    public Task<Result<bool>> HasRoleAsync(string userId, UserRole role, CancellationToken cancellationToken)
    {
        return FindByID(userId, cancellationToken).Map(user => user.Role == role);
    }
    
}
