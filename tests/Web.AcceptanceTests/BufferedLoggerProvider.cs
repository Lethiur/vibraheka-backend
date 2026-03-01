using Microsoft.Extensions.Logging;

namespace VibraHeka.Web.AcceptanceTests;

public sealed class BufferedLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new BufferedLogger(categoryName);

    public void Dispose()
    {
    }

    private sealed class BufferedLogger(string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            string timestamp = DateTimeOffset.UtcNow.ToString("O");
            string line = exception is null
                ? $"{timestamp} [{logLevel}] {categoryName}: {message}"
                : $"{timestamp} [{logLevel}] {categoryName}: {message}{Environment.NewLine}{exception}";

            TestLogBuffer.Add(line);
        }
    }
}
