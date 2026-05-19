namespace VibraHeka.Application.Abstractions.Transactions;

/// <summary>
/// Represents a batch of transactional write operations, enabling grouped modifications with an assigned idempotency key
/// for ensuring compliance with idempotency requirements in distributed systems.
/// </summary>
public sealed class TransactionalWriteBatch
{
    /// <summary>
    /// Represents the collection of transactional write operations within the scope of a
    /// <see cref="TransactionalWriteBatch"/>. Allows grouped transactional modifications to be added
    /// and executed in a consistent manner, ensuring compliance with application rules and idempotency.
    /// </summary>
    public List<ITransactionalWriteOperation> Operations { get; private set; } = [];

    /// <summary>
    /// Gets the unique key used to ensure the idempotency of a
    /// <see cref="TransactionalWriteBatch"/> in distributed systems.
    /// This key is used to prevent the unintended duplication of
    /// transactional operations by uniquely identifying a batch within a system.
    /// </summary>
    public string IdempotencyKey { get; }

    /// <summary>
    /// Represents a batch of transactional write operations, enabling atomic grouping of multiple
    /// write operations in distributed systems. This class ensures idempotency through the use of
    /// a unique idempotency key, preventing duplicate execution of the same operations.
    /// </summary>
    public TransactionalWriteBatch(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key cannot be empty.", nameof(idempotencyKey));

        IdempotencyKey = idempotencyKey;
    }

    /// <summary>
    /// Adds a transactional write operation to the batch, allowing it to be executed as part of a grouped set of operations.
    /// Ensures that the operation is not null before adding it to the internal collection.
    /// </summary>
    /// <param name="operation">The transactional write operation to be added to the batch.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="operation"/> is null.</exception>
    public void Add(ITransactionalWriteOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        Operations.Add(operation);
    }

    /// <summary>
    /// Adds a collection of transactional write operations to the batch, allowing them to be executed as part
    /// of a grouped set of operations. Ensures that each operation in the collection is not null before adding
    /// it to the internal batch.
    /// </summary>
    /// <param name="operations">The collection of transactional write operations to be added to the batch.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided <paramref name="operations"/> is null or contains a null operation.
    /// </exception>
    public void AddRange(IEnumerable<ITransactionalWriteOperation> operations)
    {
        foreach (var operation in operations)
        {
            Add(operation);
        }
    }
}
