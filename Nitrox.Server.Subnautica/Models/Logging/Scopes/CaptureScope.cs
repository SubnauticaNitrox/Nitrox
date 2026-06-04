using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.Logging.Scopes;

/// <summary>
///     Captures the logs written during this scope.<br />
///     If a <see cref="Logger"/> is given, captured logs will be logged as information when this scope is disposed.<br />
///     <br />
///     Captured logs can be retrieved via <see cref="CaptureScope.Logs" /> or <see cref="ToString"/>.
/// </summary>
internal sealed record CaptureScope : IDisposable
{
    private readonly List<string> logs = [];

    /// <param name="logger">The logger used to output captured logs when this scope is disposed. Or null if logs should remain hidden.</param>
    public CaptureScope(ILogger? logger)
    {
        Logger = logger;
    }

    public IDisposable? InnerDisposable { get; set; }

    /// <summary>
    ///     Gets the captured logs during this scope.
    /// </summary>
    public IReadOnlyCollection<string> Logs => logs;

    private ILogger? Logger { get; init; }

    /// <summary>
    ///     Adds the log to this scope.
    /// </summary>
    public void Capture(string log) => logs.Add(log);

    public override string ToString() => string.Join("", Logs).TrimEnd('\n');

    public void Dispose()
    {
        InnerDisposable?.Dispose();
        Logger?.ZLogInformation($"{this}");
    }
}
