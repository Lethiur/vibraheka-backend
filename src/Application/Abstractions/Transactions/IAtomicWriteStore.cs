using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Abstractions.Transactions;

/// <summary>
/// Defines an abstraction for atomic write operations, facilitating the execution of multiple
/// transactional actions as a single, indivisible unit of work. This interface ensures that
/// all operations within a transactional batch are committed together, maintaining atomicity
/// and preserving consistency in distributed systems.
/// </summary>
public interface IAtomicWriteStore
{
    /// <summary>
    /// Commits a batch of transactional write operations atomically, ensuring all operations
    /// within the batch are executed as a single unit of work. In case of failures, the execution
    /// retains consistency by preventing partial modifications.
    /// </summary>
    /// <param name="batch">
    /// A <see cref="TransactionalWriteBatch"/> containing the grouped write operations to be executed atomically.
    /// The batch ensures idempotency compliance using a unique idempotency key.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe cancellation requests while committing the batch.
    /// This allows for the operation to be canceled externally.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing an asynchronous operation that resolves to a <see cref="Result{Unit}"/>.
    /// The result indicates success or failure of the commit operation.
    /// </returns>
    Task<Result<Unit>> CommitAsync(TransactionalWriteBatch batch, CancellationToken cancellationToken);
}
