using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Configuration;
using VibraHeka.Application.Common.Interfaces;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public class UserRepository : IUserRepository
{
    private readonly Table _table;

    public UserRepository(IAmazonDynamoDB client, IConfiguration config)
    {
        TableBuilder builder = new TableBuilder(client, config["Dynamo:UsersTable"]);
        _table = builder.Build();
    }

    public async Task AddAsync(User user)
    {
        var doc = new Document
        {
            ["Id"] = user.Id,
            ["Email"] = user.Email,
            ["FullName"] = user.FullName,
            ["CognitoId"] = user.CognitoId
        };

        await _table.PutItemAsync(doc);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        QueryFilter filter = new();
        filter.AddCondition("Email", QueryOperator.Equal, email);
        var search = _table.Query(filter);
        var results = await search.GetNextSetAsync();
        return results.Count > 0;
    }
}
