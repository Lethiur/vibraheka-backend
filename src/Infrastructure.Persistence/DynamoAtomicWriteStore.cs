using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using VibraHeka.Application.Abstractions.Transactions;
using VibraHeka.Domain.Common.Errors;

namespace Infrastructure.Persistence;

/// <summary>
/// The <c>DynamoAtomicWriteStore</c> class provides functionality for managing atomic
/// transactional writes to an Amazon DynamoDB database. It serves as an implementation
/// of the <see cref="IAtomicWriteStore"/> interface, enabling precise control over
/// batching and committing write operations within the constraints of DynamoDB.
/// </summary>
/// <remarks>
/// This class ensures atomicity of write operations by utilizing DynamoDB's
/// transactional write capabilities. It verifies batch sizes to comply with DynamoDB's
/// maximum operation constraints and handles error handling for transactional failures.
/// </remarks>
public class DynamoAtomicWriteStore(IDynamoDBContext Context, ILogger<DynamoAtomicWriteStore> Logger) : IAtomicWriteStore
{
    /// <summary>
    /// Represents the maximum number of operations that can be included in a single
    /// transactional write request to Amazon DynamoDB.
    /// </summary>
    /// <remarks>
    /// This constant enforces the constraint imposed by DynamoDB, which allows a maximum
    /// of 100 operations in a single transactional write request. Exceeding this limit
    /// results in an error, so this value is used to validate batch sizes and ensure
    /// compliance with the DynamoDB API.
    /// </remarks>
    private const int MaxDynamoDbTransactionOperations = 100;

    /// <summary>
    /// Executes a transactional write operation in DynamoDB.
    /// </summary>
    /// <param name="batch">
    /// The <see cref="TransactionalWriteBatch"/> containing the operations to be executed as part of the transaction.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the result of the transactional write.
    /// The result contains a <see cref="Result{T}"/> that is either successful or contains an error.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="batch"/> parameter is null.
    /// </exception>
    public async Task<Result<Unit>> CommitAsync(TransactionalWriteBatch batch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(batch, nameof(batch));

        if (batch.Operations.Count > MaxDynamoDbTransactionOperations)
        {
            Logger.LogWarning("Batch contains too many operations for DynamoDB transaction");
            return Result.Failure<Unit>(DomainErrors.GenericError);
        }

        ITransactWrite[] transactWriteItems = batch.Operations.Select(ToDynamoOperation).ToArray();

        try
        {
            await Context.ExecuteTransactWriteAsync(transactWriteItems, cancellationToken);
            return Unit.Value;
        }
        catch (AmazonDynamoDBException ex)
        {
            Logger.LogError(ex, "Failed to commit transactional write batch with idempotency key {IdempotencyKey}", batch.IdempotencyKey);
            return Result.Failure<Unit>(DomainErrors.GenericError);
        }

    }

    /// <summary>
    /// Converts a transactional write operation into a DynamoDB specific transactional write item.
    /// </summary>
    /// <param name="operation">
    /// An instance of <see cref="ITransactionalWriteOperation"/> representing the transactional operation to be converted.
    /// </param>
    /// <returns>
    /// A <see cref="TransactWriteItem"/> that represents the operation in DynamoDB's transactional write format.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the provided <paramref name="operation"/> is not of type <see cref="DynamoTransactionalWriteOperation"/>.
    /// </exception>
    private static ITransactWrite ToDynamoOperation(
        ITransactionalWriteOperation operation)
    {
        if (operation is not DynamoTransactionalWriteOperation dynamoOperation)
        {
            throw new InvalidOperationException(
                $"Unsupported transactional operation type: {operation.GetType().Name}");
        }

        return dynamoOperation.Item;
    }
}

