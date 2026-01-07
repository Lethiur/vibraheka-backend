using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public abstract class GenericDynamoRepository<T>(IDynamoDBContext context,  IConfiguration config, string tableConfigKey)
{
    /// <summary>
    /// Retrieves an entity of type T from the DynamoDB table by its unique ID.
    /// </summary>
    /// <param name="ID">The unique identifier of the entity to be retrieved.</param>
    /// <returns>
    /// A <see cref="Maybe{T}"/> containing the entity if found, or <see cref="Maybe{T}.None"/> if the entity is not found or an error occurs.
    /// </returns>
    protected async Task<Result<T>> FindByID(string ID)
    {
        LoadConfig configuration = new() { OverrideTableName = config[tableConfigKey] };
        try
        {
            T? model = await context.LoadAsync<T>(ID, configuration);
            return model;
        }
        catch (Exception e)
        {
            return Result.Failure<T>(HandleError(e));
        }
    }

    /// <summary>
    /// Saves an entity of type T to the DynamoDB table.
    /// </summary>
    /// <param name="entity">The entity of type T to be saved to the table.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> representing the success or failure of the save operation.
    /// In case of failure, it contains an error message.
    /// </returns>
    protected async Task<Result<Unit>> Save(T entity)
    {
        SaveConfig saveConfig = new() { OverrideTableName = config[tableConfigKey] };

        try
        {
            await context.SaveAsync(entity, saveConfig);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return Result.Failure<Unit>(HandleError(e));
        }
    }
    
    /// <summary>
    /// Handles an exception that occurs during execution of repository operations.
    /// </summary>
    /// <param name="ex">The exception to be handled.</param>
    protected abstract string HandleError(Exception ex);
}
