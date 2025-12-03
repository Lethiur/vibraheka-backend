using System.Net;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next)
{
  

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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {

        var errorResponse = new
        {
            success = false,
            error = exception.Message
        };

        string json = JsonSerializer.Serialize(errorResponse);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 400;

        return context.Response.WriteAsync(json);
    }
}
