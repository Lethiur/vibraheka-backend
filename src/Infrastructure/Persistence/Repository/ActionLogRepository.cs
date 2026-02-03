using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Repository for managing action logs in a DynamoDB table. This class extends the functionality of
/// <see cref="GenericDynamoRepository{T}"/> and provides specific methods for handling action logs.
/// </summary>
/// <remarks>
/// The repository uses an implementation of <see cref="IDynamoDBContext"/> for CRUD operations
/// and retrieves the table configuration from <see cref="AWSConfig.ActionLogTable"/>.
/// It implements <see cref="IActionLogRepository"/> for retrieving user-specific action logs based on action types.
/// </remarks>
public class ActionLogRepository(IDynamoDBContext context, AWSConfig config)
    : GenericDynamoRepository<ActionLogDBModel>(context, config.ActionLogTable), IActionLogRepository
{
    protected override string HandleError(Exception ex)
    {
        return ex.Message;
    }

    /// <summary>
    /// Retrieves the action log for a specific user and action type from the DynamoDB repository.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose action log is to be retrieved.</param>
    /// <param name="action">The type of action for which the log is being requested.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if necessary.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{T}"/>
    /// instance with an <see cref="ActionLogEntity"/> if the retrieval is successful, or an error if it fails.
    /// </returns>
    public Task<Result<ActionLogEntity>> GetActionLogForUser(string userID, ActionType action,
        CancellationToken cancellationToken)
    {
        return FindByIdAndRangeKey(userID, action, cancellationToken)
            .Ensure(record => record != null, ActionLogErrors.ActionLogNotFound)
            .MapTry(a => a.ToDomain());
    }

    /// <summary>
    /// Saves the provided action log to the DynamoDB repository.
    /// </summary>
    /// <param name="actionLog">The action log entity to be saved.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous save operation. The task result contains
    /// a <see cref="Result{T}"/> instance with the saved <see cref="ActionLogEntity"/> if
    /// the operation is successful, or an error if it fails.
    /// </returns>
    public Task<Result<ActionLogEntity>> SaveActionLog(ActionLogEntity actionLog, CancellationToken cancellationToken)
    {
        return Save(ActionLogDBModel.FromDomain(actionLog), cancellationToken).Map(a => actionLog);
    }
}
