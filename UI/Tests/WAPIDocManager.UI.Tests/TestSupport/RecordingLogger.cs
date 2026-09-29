using Microsoft.Extensions.Logging;

namespace WAPIDocManager.UI.Tests.TestSupport;

/// <summary>
/// Logger fittizio che registra le voci scritte, per verificare che un avviso ci sia (o non ci sia).
/// </summary>
/// <remarks>Nei test dei componenti che loggano, l'alternativa è NullLogger quando l'esito non interessa.</remarks>
public sealed class RecordingLogger<T> : ILogger<T>
{
    private readonly List<(LogLevel Level, string Message)> _entries = [];

    public IReadOnlyList<(LogLevel Level, string Message)> Entries => _entries;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _entries.Add((logLevel, formatter(state, exception)));
    }
}
