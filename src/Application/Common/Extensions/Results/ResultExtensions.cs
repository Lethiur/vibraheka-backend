using CSharpFunctionalExtensions;

namespace VibraHeka.Application.Common.Extensions.Results;

/// <summary>
/// A static class that provides extension methods for handling Results in a functional programming style.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes a compensation function when the provided predicate evaluates to true and the initial result is a failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">A task representing the result to evaluate and potentially compensate.</param>
    /// <param name="compensationPredicate">A function that determines whether the compensation should be applied based on the error message.</param>
    /// <param name="compensateFunc">A function to execute if the compensation predicate is true, which returns a new result.</param>
    /// <returns>A task representing the original result if successful, or the compensated result if the predicate is true and the initial result was a failure.</returns>
    public static async Task<Result<T>> OnFailureCompensateWhen<T>(this Task<Result<T>> result,
        Func<string, bool> compensationPredicate, Func<string, Task<Result<T>>> compensateFunc)
    {
        Result<T> taskResult = await result;

        if (taskResult.IsSuccess)
        {
            return taskResult;
        }

        if (!compensationPredicate(taskResult.Error))
        {
            return Result.Failure<T>(taskResult.Error);
        }
        
        return await compensateFunc(taskResult.Error);
    }
}
