using Amazon.XRay.Recorder.Core;
using VibraHeka.Domain.Common.Interfaces;

namespace VibraHeka.Infrastructure.Tracer;

public class XRayTracer : ITracer
{
    public IDisposable BeginSegment(string name)
    {
        if (AWSXRayRecorder.Instance.GetEntity() != null)
        {
            AWSXRayRecorder.Instance.BeginSubsegment(name);
            return new Scope();
        }

        return NullScope.Instance;
    }

    public void AddException(Exception ex)
    {
        if (AWSXRayRecorder.Instance.GetEntity() != null)
            AWSXRayRecorder.Instance.AddException(ex);
    }

    public string? GetTraceId()
        => AWSXRayRecorder.Instance.GetEntity()?.TraceId;

    private class Scope : IDisposable
    {
        public void Dispose() =>
            AWSXRayRecorder.Instance.EndSubsegment();
    }

    private class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
