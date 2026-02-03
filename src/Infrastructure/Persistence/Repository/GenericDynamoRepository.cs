using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public abstract class GenericDynamoRepository<T>(IDynamoDBContext context, string tableConfigKey)
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
        LoadConfig configuration = new() { OverrideTableName = tableConfigKey };
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
    /// Retrieves an entity of type T from the DynamoDB table by its unique ID and range key.
    /// </summary>
    /// <param name="idValue">The unique identifier of the entity.</param>
    /// <param name="rangeKeyValue">The range key associated with the entity.</param>
    /// <param name="cancellationToken">The token used to halt the operation</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the entity if found, or a failure result with an error message if the entity is not found or an error occurs.
    /// </returns>
    protected async Task<Result<T>> FindByIdAndRangeKey(string idValue, object rangeKeyValue, CancellationToken cancellationToken)
    {
        LoadConfig configuration = new() { OverrideTableName = tableConfigKey };
        try
        {
            T? model = await context.LoadAsync<T>(idValue, rangeKeyValue, configuration, cancellationToken);
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
    /// <param name="token">The cancellation token to preemptively cancel the operation if needed.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> representing the success or failure of the save operation.
    /// In case of failure, it contains an error message.
    /// </returns>
    protected async Task<Result<Unit>> Save(T entity, CancellationToken token = default)
    {
        SaveConfig saveConfig = new() { OverrideTableName = tableConfigKey };

        try
        {
            await context.SaveAsync(entity, saveConfig, token);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return Result.Failure<Unit>(HandleError(e));
        }
    }

    /// <summary>
    /// Retrieves all entities of type T from the DynamoDB table.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{IEnumerable{T}}"/> containing a collection of all entities if the operation is successful,
    /// or a failure result with an error message if an error occurs.
    /// </returns>
    protected async Task<Result<IEnumerable<T>>> GetAll(CancellationToken cancellationToken)
    {
        ScanConfig configuration = new() { OverrideTableName = tableConfigKey };

        try
        {
            IAsyncSearch<T> asyncSearch = context.ScanAsync<T>(Enumerable.Empty<ScanCondition>(), configuration);
            List<T> models = await asyncSearch.GetRemainingAsync(cancellationToken);
            return models;
        }
        catch (Exception e)
        {
            return Result.Failure<IEnumerable<T>>(HandleError(e));
        }
    }
    /// <summary>
    /// Handles an exception that occurs during execution of repository operations.
    /// </summary>
    /// <param name="ex">The exception to be handled.</param>
    protected abstract string HandleError(Exception ex);
}
