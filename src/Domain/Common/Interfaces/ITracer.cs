namespace VibraHeka.Domain.Common.Interfaces;

public interface ITracer
{
    IDisposable BeginSegment(string name);
    void AddException(Exception ex);
    
    string? GetTraceId();
}
