using CSharpFunctionalExtensions;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Domain.Common.Interfaces;

public interface IActionLogRepository 
{
    /// <summary>
    /// Retrieves the action log for a specific user based on the provided user ID and action type.
    /// </summary>
    /// <param name="userID">The unique identifier of the user whose action log is being requested.</param>
    /// <param name="action">The specific action type to filter the action log.</param>
    /// <param name="cancellationToken">The token used to halt the operations</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{T}"/> wrapping an <see cref="ActionLogEntity"/>
    /// if the action log is successfully retrieved, or an error result otherwise.</returns>
    public Task<Result<ActionLogEntity>> GetActionLogForUser(string userID, ActionType action, CancellationToken cancellationToken);


    /// <summary>
    /// Saves the provided action log entity to the repository.
    /// </summary>
    /// <param name="actionLog">An instance of <see cref="ActionLogEntity"/> representing the action log to be saved.</param>
    /// <param name="cancellationToken">The token used to halt the operations</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Result{T}"/> wrapping the saved
    /// <see cref="ActionLogEntity"/> if the operation is successful, or an error result otherwise.</returns>
    public Task<Result<ActionLogEntity>> SaveActionLog(ActionLogEntity actionLog, CancellationToken cancellationToken);
}
