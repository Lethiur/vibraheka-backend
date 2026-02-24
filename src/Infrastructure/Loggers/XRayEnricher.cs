using Amazon.XRay.Recorder.Core;
using Serilog.Core;
using Serilog.Events;

namespace VibraHeka.Web.Logging;

public class XRayEnricher: ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var traceId = AWSXRayRecorder.Instance.GetEntity()?.TraceId;
        if (!string.IsNullOrEmpty(traceId))
        {
            var property = propertyFactory.CreateProperty("TraceId", traceId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
