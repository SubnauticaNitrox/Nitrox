using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.Logging.Scopes;

/// <summary>
///     Captures the logs written during this scope. Captured logs can be retrieved via
///     <see cref="CaptureScope.Logs" />.
/// </summary>
internal sealed record CaptureScope : IDisposable
{
    private readonly List<string> logs = [];
    public IDisposable? InnerDisposable { get; set; }

    /// <summary>
    ///     Gets the captured logs during this scope.
    /// </summary>
    public IReadOnlyCollection<string> Logs => logs;

    /// <summary>
    ///     Adds the log to this scope.
    /// </summary>
    public void Capture(string log) => logs.Add(log);

    public void Dispose() => InnerDisposable?.Dispose();
}
