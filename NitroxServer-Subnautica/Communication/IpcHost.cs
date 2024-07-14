using System;
using System.IO;
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
    private readonly CancellationTokenSource commandReadCancellation;
    private readonly NamedPipeServerStream server = new($"Nitrox Server {NitroxEnvironment.CurrentProcessId}", PipeDirection.In, 1);

    private IpcHost(CancellationTokenSource commandReadCancellation)
    {
        this.commandReadCancellation = commandReadCancellation;
    }

    public static IpcHost StartReadingCommands(Action<string> onCommandReceived, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(onCommandReceived);

        IpcHost host = new(CancellationTokenSource.CreateLinkedTokenSource(ct));
        Thread thread = new(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    string command = await host.ReadStringAsync(ct);
                    onCommandReceived(command);
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            }
        });
        thread.IsBackground = true;
        thread.Start();
        return host;
    }

    public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
    {
        if (!await WaitForConnection())
        {
            return "";
        }

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

    private async Task<bool> WaitForConnection()
    {
        if (server.IsConnected)
        {
            return true;
        }
        try
        {
            await server.WaitForConnectionAsync();
            return true;
        }
        catch (IOException)
        {
            try
            {
                server.Disconnect();
            }
            catch (Exception)
            {
                // ignored
            }
        }
        return false;
    }
}
