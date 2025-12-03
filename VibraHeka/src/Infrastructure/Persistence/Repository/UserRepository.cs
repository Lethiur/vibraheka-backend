using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class UserRepository(IDynamoDBContext context) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        SaveConfig saveConfig = new SaveConfig()
        {
            OverrideTableName = "VibraHeka-users",
            
        };
        
        await context.SaveAsync(user, saveConfig);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        QueryConfig queryConfig = new()
        {
            IndexName = "EmailIndex",
            OverrideTableName = "VibraHeka-users",
        };
        
        List<User>? results = await context.QueryAsync<User>(email, queryConfig).GetRemainingAsync();
        return results?.Count > 0;
    }
}
