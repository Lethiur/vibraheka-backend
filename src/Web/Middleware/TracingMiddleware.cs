using Amazon.XRay.Recorder.Core;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Web.Middleware;

public class TracingMiddleware(RequestDelegate next, ITracer tracer, ILogger<TracingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            var entity = AWSXRayRecorder.Instance.GetEntity();
            if (entity?.Aws != null)
            {
                var logGroupMetadata = new List<object>
                {
                    new { log_group = "/my-app/logs" }
                };

                entity.Aws["cloudwatch_logs"] = logGroupMetadata;
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
            tracer.AddException(ex);
        }

        await next(context);
    }
}
