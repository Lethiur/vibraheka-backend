using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Interfaces.Codes;
using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Mappers;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Provides methods to interact with DynamoDB for managing verification codes associated with users.
/// </summary>
public class VerificationCodesRepository(IDynamoDBContext context,  AWSConfig config, VerificationCodeEntityMapper mapper) : ICodeRepository
{
    
    public async Task<Result<VerificationCodeEntity>> GetCodeFor(string email)
    {
        LoadConfig loadConfig = new()
        {
            OverrideTableName = config.CodesTable,
        };

        VerificationCodeDBModel? results = await context.LoadAsync<VerificationCodeDBModel>(email,  loadConfig);

        if (results == null)
        {
            return Result.Failure<VerificationCodeEntity>("No codes found for user with Email: " + email);
        }
        
        return mapper.ToDomain(results);
    }
}
