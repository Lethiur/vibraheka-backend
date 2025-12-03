using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class UserRepository(IDynamoDBContext context) : IUserRepository
{
    public async Task<Result<string>> AddAsync(User user)
    {
        SaveConfig saveConfig = new()
        {
            OverrideTableName = "VibraHeka-users",
        };
        
        await context.SaveAsync(user, saveConfig);
        return user.Id;
    }

    public async Task<Result<bool>> ExistsByEmailAsync(string email)
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
