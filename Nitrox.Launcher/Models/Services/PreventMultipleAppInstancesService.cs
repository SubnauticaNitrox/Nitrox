using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Services;

internal sealed class PreventMultipleAppInstancesService : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        if (App.allowInstances || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Task.CompletedTask;
        }

        try
        {
            using ProcessEx process = ProcessEx.GetFirstProcess("Nitrox.Launcher", process => process.Id != Environment.ProcessId && process.IsRunning);
            if (process is not null)
            {
                process.SetForegroundWindowAndRestore();
                Environment.Exit(0);
            }
        }
        catch (Exception)
        {
            // Ignore
        }
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
