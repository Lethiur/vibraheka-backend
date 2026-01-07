using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using VibraHeka.Domain.Common.Interfaces.Codes;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Provides methods to interact with DynamoDB for managing verification codes associated with users.
/// </summary>
public class VerificationCodesRepository(IDynamoDBContext context,  IConfiguration config) : ICodeRepository
{
    
    public async Task<Result<VerificationCodeEntity>> GetCodeFor(string email)
    {
        LoadConfig loadConfig = new()
        {
            OverrideTableName = config["Dynamo:CodesTable"],
        };

        VerificationCodeEntity? results = await context.LoadAsync<VerificationCodeEntity>(email,  loadConfig);

        if (results == null)
        {
            return Result.Failure<VerificationCodeEntity>("No codes found for user with Email: " + email);
        }
        
        return results;
    }
}
