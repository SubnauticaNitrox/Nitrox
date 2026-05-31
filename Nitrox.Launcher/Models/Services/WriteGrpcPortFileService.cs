using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Nitrox.Model.Constants;
using Nitrox.Model.Logger;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Writes the gRPC connection info in a file that Nitrox servers can access. On Windows, writes the named pipe name. On Linux, writes the port number.
/// </summary>
internal class WriteGrpcPortFileService(IServer server) : IHostedLifecycleService
{
    private readonly string filePath = Path.Combine(Path.GetTempPath(), LauncherConstants.GRPC_LISTEN_PORT_TEMP_FILE_NAME);
    private readonly IServer server = server;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        string connectionInfo;
        if (OperatingSystem.IsWindows())
        {
            // On Windows, write the named pipe name
            connectionInfo = LauncherConstants.GRPC_NAMED_PIPE_NAME;
        }
        else
        {
            // On Non-Windows, write the port number.
            // We expect only one port to be known by .NET server (kestrel). If there are more, we need to refactor this code to ONLY write the gRPC port.
            IServerAddressesFeature? addressFeature = server.Features.Get<IServerAddressesFeature>();
            int grpcPort = addressFeature.Addresses.Select(a => new Uri(a).Port).First();
            connectionInfo = grpcPort.ToString();
        }

        const int INITIAL_ATTEMPTS = 10;
        int attempts = INITIAL_ATTEMPTS;
        while (attempts-- > 0)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, connectionInfo, cancellationToken);
                break;
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to write gRPC connection info (attempt {INITIAL_ATTEMPTS - attempts}/{INITIAL_ATTEMPTS}): {ex.Message}");
                await Task.Delay(500, cancellationToken);
            }
        }
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception)
        {
            // ignored
        }
        return Task.CompletedTask;
    }
}
