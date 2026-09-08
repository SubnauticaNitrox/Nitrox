using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Services.Core;

public interface IProgressReporter
{
    /// <summary>
    ///     Reports loading progress to any connected management clients.
    /// </summary>
    /// <param name="stage">Description of the current loading stage (e.g., "Loading entities", "Initializing world")</param>
    /// <param name="progress">Progress value between 0.0 and 1.0</param>
    Task ReportProgressAsync(string stage, float progress);

    IAsyncEnumerable<ProgressUpdate> ReadAllAsync(CancellationToken cancellationToken);

    public record ProgressUpdate(string Stage, float Progress);
}
