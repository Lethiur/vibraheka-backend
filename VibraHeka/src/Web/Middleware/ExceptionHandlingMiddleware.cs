using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.Middleware;

/// <summary>
/// Middleware for handling exceptions during the request pipeline.
/// Captures unhandled exceptions that occur during the processing of HTTP requests
/// and converts them into appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Processes an HTTP request by invoking the next delegate in the middleware pipeline.
    /// Captures any unhandled exceptions that occur during the request processing and passes
    /// them to the exception handler for generating an appropriate HTTP response.
    /// </summary>
    /// <param name="context">The context of the current HTTP request being processed.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles an exception by constructing an appropriate JSON response and writing it
    /// to the HTTP response stream. Ensures the response includes an error message and a
    /// status code of 400.
    /// </summary>
    /// <param name="context">The context of the current HTTP request.</param>
    /// <param name="exception">The exception that was thrown during request processing.</param>
    /// <returns>A task that represents the asynchronous operation of writing the response.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = 400;
        return context.Response.WriteAsJsonAsync(ResponseEntity.FromError(exception.Message));
    }
}
