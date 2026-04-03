using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VibraHeka.Infrastructure.Entities;

namespace VibraHeka.Infrastructure.Middlewares;

public class TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger, AWSLoggingConfig loggingConfig)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            Entity? entity = AWSXRayRecorder.Instance.GetEntity();
            if (entity?.Aws != null)
            {
                entity.Aws["cloudwatch_logs"] = new List<object> { new { log_group = loggingConfig.LogGroup } };
            }
            else
            {
                logger.LogDebug("Skipping tracing enrichment for {Method} {Path}: no active X-Ray entity",
                    context.Request.Method, context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Tracing middleware enrichment failed for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        await next(context);
    }
}
