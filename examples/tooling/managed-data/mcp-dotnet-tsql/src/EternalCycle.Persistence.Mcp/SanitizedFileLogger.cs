using Microsoft.Extensions.Logging;

namespace EternalCycle.Persistence.Mcp;

public sealed class SanitizedFileLoggerProvider(string filePath) : ILoggerProvider
{
    private readonly object sync = new();
    private readonly string destination = Path.GetFullPath(filePath);

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    public void Dispose()
    {
    }

    private void Write(
        LogLevel level,
        string category,
        EventId eventId,
        string message,
        Exception? exception)
    {
        var directory = Path.GetDirectoryName(destination);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var safeMessage = SingleLine(message);
        var safeCategory = SingleLine(category);
        var exceptionType = exception is null ? string.Empty : $" exception={exception.GetType().Name}";
        var line = $"{DateTimeOffset.UtcNow:O} level={level} event={eventId.Id} category={safeCategory}{exceptionType} message={safeMessage}{Environment.NewLine}";
        lock (sync)
        {
            File.AppendAllText(destination, line);
        }
    }

    private static string SingleLine(string value)
    {
        var sanitized = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return sanitized.Length <= 2000 ? sanitized : sanitized[..2000];
    }

    private sealed class FileLogger(SanitizedFileLoggerProvider owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                owner.Write(logLevel, category, eventId, formatter(state, null), exception);
            }
        }
    }
}
