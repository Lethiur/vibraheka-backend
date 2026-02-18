using Amazon.XRay.Recorder.Core;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Web.Middleware;

public class TracingMiddleware(RequestDelegate next, ITracer tracer)
{

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var entity = AWSXRayRecorder.Instance.GetEntity();
            var logGroupMetadata = new List<object> 
            { 
                new { log_group = "/my-app/logs" } 
            };

            entity.Aws["cloudwatch_logs"] = logGroupMetadata;
            await next(context);
        }
        catch (Exception ex)
        {
            tracer.AddException(ex);
            throw;
        }
    }
}
