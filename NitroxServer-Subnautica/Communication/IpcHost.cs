using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Helper;

namespace NitroxServer_Subnautica.Communication;

/// <summary>
///     Exposes an IPC channel for other local processes to communicate with the server.
/// </summary>
public class IpcHost : IDisposable
{
    private readonly CancellationTokenSource commandReadCancellation = new();
    private readonly NamedPipeServerStream server = new($"Nitrox Server {NitroxEnvironment.CurrentProcessId}", PipeDirection.In, 1);

    public static IpcHost StartReadingCommands(Action<string> onCommandReceived)
    {
        ArgumentNullException.ThrowIfNull(onCommandReceived);

        IpcHost host = new();
        Thread thread = new(async () =>
        {
            while (!host.commandReadCancellation.IsCancellationRequested)
            {
                string command = await host.ReadStringAsync(host.commandReadCancellation.Token);
                onCommandReceived(command);
            }
        });
        thread.IsBackground = true;
        thread.Start();
        return host;
    }

    public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
    {
        await WaitForConnection();

        byte[] sizeBytes = new byte[4];
        await server.ReadExactlyAsync(sizeBytes, cancellationToken);
        byte[] stringBytes = new byte[BitConverter.ToUInt32(sizeBytes)];
        await server.ReadExactlyAsync(stringBytes, cancellationToken);
        return Encoding.UTF8.GetString(stringBytes);
    }

    public void Dispose()
    {
        commandReadCancellation?.Cancel();
        server.Dispose();
    }

    private async Task WaitForConnection()
    {
        if (server.IsConnected)
        {
            return;
        }
        await server.WaitForConnectionAsync();
    }
}
