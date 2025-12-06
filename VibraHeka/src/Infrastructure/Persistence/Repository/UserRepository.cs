using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class UserRepository(IDynamoDBContext context, IConfiguration config) : IUserRepository
{
    public async Task<Result<string>> AddAsync(User user)
    {
        SaveConfig saveConfig = new()
        {
            OverrideTableName = config["Dynamo:UsersTable"],
        };
        
        await context.SaveAsync(user, saveConfig);
        return user.Id;
    }

    public async Task<Result<bool>> ExistsByEmailAsync(string email)
    {
        QueryConfig queryConfig = new()
        {
            IndexName = "EmailIndex",
            OverrideTableName = config["Dynamo:UsersTable"]
        };
        
        List<User>? results = await context.QueryAsync<User>(email, queryConfig).GetRemainingAsync();
        return results?.Count > 0;
    }
}
