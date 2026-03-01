using Microsoft.Extensions.Logging;

namespace VibraHeka.Infrastructure.IntegrationTests;

public sealed class NUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NUnitLogger(categoryName);

    public void Dispose()
    {
    }

    private sealed class NUnitLogger(string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            string timestamp = DateTimeOffset.UtcNow.ToString("O");

            if (exception is null)
            {
                TestContext.Progress.WriteLine($"{timestamp} [{logLevel}] {categoryName}: {message}");
                return;
            }

            TestContext.Progress.WriteLine(
                $"{timestamp} [{logLevel}] {categoryName}: {message}{Environment.NewLine}{exception}");
        }
    }
}
