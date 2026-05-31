using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.Hosting;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Avalonia starts its own loop after .NET app host. This service will close Avalonia too when the app host gets a
///     request
///     to close.
/// </summary>
/// <remarks>
///     This service might not be needed later if Avalonia gives access to its <see cref="CancellationTokenSource" />.
/// </remarks>
internal sealed class StopAvaloniaOnHostExitService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => Dispatcher.UIThread.Invoke(() => App.Instance.AppWindow.CloseByCode()));
        return Task.CompletedTask;
    }
}
