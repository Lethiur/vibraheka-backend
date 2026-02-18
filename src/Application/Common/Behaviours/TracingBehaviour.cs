using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Logging;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Application.Common.Behaviours;

public class TracingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ITracer _tracer;
    private readonly ILogger<TracingBehaviour<TRequest, TResponse>> _logger;

    public TracingBehaviour(ITracer tracer, ILogger<TracingBehaviour<TRequest, TResponse>> logger)
    {
        _tracer = tracer;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        AWSXRayRecorder.Instance.AddAnnotation("RequestType", typeof(TRequest).Name);
        AWSXRayRecorder.Instance.AddAnnotation("TraceId", _tracer.GetTraceId());
        AWSXRayRecorder.Instance.AddMetadata("RequestPayload", request);
        try
        {
            return await next(ct);
        }
        catch (Exception ex)
        {
            _tracer.AddException(ex);
            _logger.LogError(ex, "Request failed");
            throw;
        }
    }
}
