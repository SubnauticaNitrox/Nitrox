using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.Logging.Scopes;

/// <summary>
///     Captures the logs written during this scope. Captured logs can be retrieved via
///     <see cref="CaptureScope.Logs" />.
/// </summary>
public record CaptureScope : IDisposable
{
    public IDisposable? InnerDisposable { get; set; }

    /// <summary>
    ///     Gets the captured logs during this scope.
    /// </summary>
    public List<string> Logs { get; } = [];

    public void Dispose() => InnerDisposable?.Dispose();
}
