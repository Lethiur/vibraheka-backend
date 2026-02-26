using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Infrastructure.Exceptions;

namespace VibraHeka.Infrastructure.Persistence.Repository;

public abstract class GenericDynamoRepository<T>(
    IDynamoDBContext context,
    string tableConfigKey,
    ILogger<GenericDynamoRepository<T>> logger)
{
    /// <summary>
    /// Retrieves an entity of type T from the DynamoDB table by its unique ID.
    /// </summary>
    /// <param name="ID">The unique identifier of the entity to be retrieved.</param>
    /// <returns>
    /// A <see cref="Maybe{T}"/> containing the entity if found, or <see cref="Maybe{T}.None"/> if the entity is not found or an error occurs.
    /// </returns>
    protected async Task<Result<T>> FindByID(string ID, CancellationToken token)
    {
        logger.LogInformation("Retrieving entity of type {EntityType} by ID  {ID}", typeof(T).Name, ID);
        LoadConfig configuration = new() { OverrideTableName = tableConfigKey };
        try
        {
            T model = await context.LoadAsync<T>(ID, configuration, token);
            return Maybe.From(model).ToResult(GenericPersistenceErrors.NoRecordsFound).Tap(_ =>
            {
                logger.LogInformation("Successfully retrieved of type {EntityType} entity with ID: {EntityID}",
                    typeof(T).Name, ID);
            });
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
    protected async Task<Result<T>> FindByIdAndRangeKey(string idValue, object rangeKeyValue,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving entity of type {EntityType} by ID  {ID} and range key {RangeKey}", typeof(T).Name, idValue, rangeKeyValue);
        LoadConfig configuration = new() { OverrideTableName = tableConfigKey };
        try
        {
            T? model = await context.LoadAsync<T>(idValue, rangeKeyValue, configuration, cancellationToken);
            return Maybe.From(model)
                .ToResult(GenericPersistenceErrors.NoRecordsFound);
        }
        catch (Exception e)
        {
            return Result.Failure<T>(HandleError(e));
        }
    }

    /// <summary>
    /// Retrieves a single entity of type T from the DynamoDB table using a specified index name and index value.
    /// </summary>
    /// <param name="indexName">The name of the index used to query the table.</param>
    /// <param name="indexValue">The value of the index key to be used for the query.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the first entity that matches the specified index value,
    /// or an error result if no records are found or an error occurs during the operation.
    /// </returns>
    protected async Task<Result<T>> FindOneByIndex(string indexName, string indexValue,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Retrieving entity of type {EntityType} using index {IndexName} with value {IndexValue}", typeof(T).Name, indexName, indexValue);
        QueryConfig queryConfig = new() { IndexName = indexName, OverrideTableName = tableConfigKey };
        try
        {
            IAsyncSearch<T>? search = context.QueryAsync<T>(indexValue, queryConfig);
            List<T>? models = await search.GetRemainingAsync(cancellationToken);

            return Maybe.From(models)
                .ToResult(GenericPersistenceErrors.NoRecordsFound)
                .Ensure(modelsResult => modelsResult.Count > 0, GenericPersistenceErrors.NoRecordsFound)
                .Map(list => list[0]);
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
        using IDisposable? _ = logger.BeginScope(new Dictionary<string, object?>
            { ["TraceId"] = AWSXRayRecorder.Instance.GetEntity()?.Id });
        try
        {
            await context.SaveAsync(entity, saveConfig, token);
            logger.LogInformation("Successfully saved entity of type {EntityType}", typeof(T).Name);
            return Unit.Value;
        }
        catch (Exception e)
        {
            return Result.Failure<Unit>(HandleError(e));
        }
    }

    protected async Task<Result<Unit>> Delete(T entity, CancellationToken token)
    {
        DeleteConfig deleteConfig = new() { OverrideTableName = tableConfigKey };
        try
        {
            logger.LogInformation("Deleting entity of type {EntityType}", typeof(T).Name);
            await context.DeleteAsync(entity, deleteConfig, token);
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
        using IDisposable? _ = logger.BeginScope(new Dictionary<string, object?>
            { ["TraceId"] = AWSXRayRecorder.Instance.GetEntity()?.Id });
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
    private string HandleError(Exception ex)
    {
        logger.LogError(ex, "Error occurred while executing repository operation");
        return ex switch
        {
            ProvisionedThroughputExceededException => GenericPersistenceErrors.ProvisionedThroughputExceeded,
            ResourceNotFoundException => GenericPersistenceErrors.ResourceNotFound,
            ConditionalCheckFailedException => GenericPersistenceErrors.ConditionalCheckFailed,
            _ => GenericPersistenceErrors.GeneralError
        };
    }
}
