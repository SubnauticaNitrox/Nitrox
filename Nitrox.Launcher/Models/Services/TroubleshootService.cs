using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Checks if the launcher environment has problems and guides the user to solve them.
/// </summary>
internal sealed class TroubleshootService : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        if (!FileSystem.Instance.SetFullAccessToCurrentUser(NitroxUser.ExecutableRootPath))
        {
            LauncherNotifier.Warning($"{NitroxEnvironment.AppName} files might not be accessible by the game. Check that this program isn't running from a zip file, OneDrive or Program Files.");
        }
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
