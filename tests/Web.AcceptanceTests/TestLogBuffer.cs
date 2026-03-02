using System.Collections.Concurrent;

namespace VibraHeka.Web.AcceptanceTests;

public static class TestLogBuffer
{
    private static readonly ConcurrentQueue<string> Entries = new();

    public static void Add(string message) => Entries.Enqueue(message);

    public static IEnumerable<string> Drain()
    {
        while (Entries.TryDequeue(out string? entry))
        {
            yield return entry;
        }
    }

    public static void Clear()
    {
        while (Entries.TryDequeue(out _))
        {
        }
    }
}
