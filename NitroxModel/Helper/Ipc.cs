using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroxModel.Helper;

public class Ipc : IDisposable
{
    private readonly NamedPipeServerStream receivePipe;
    private readonly NamedPipeServerStream sendPipe;
    private readonly CancellationTokenSource cancellationTokenSource;
    
    private Ipc(NamedPipeServerStream receivePipe, NamedPipeServerStream sendPipe, CancellationTokenSource cancellationTokenSource)
    {
        this.receivePipe = receivePipe;
        this.sendPipe = sendPipe;
        this.cancellationTokenSource = cancellationTokenSource;
    }
    
    public static Ipc Create(string pipeName)
    {
        NamedPipeServerStream receivePipe = new($"{pipeName}_receive", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        NamedPipeServerStream sendPipe = new($"{pipeName}_send", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

        CancellationTokenSource cancellationTokenSource = new();

        return new Ipc(receivePipe, sendPipe, cancellationTokenSource);
    }

    public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
    {
        if (!receivePipe.IsConnected)
        {
            await receivePipe.WaitForConnectionAsync(cancellationToken);
        }

        using MemoryStream memoryStream = new();
        byte[] buffer = new byte[1024];
        int bytesRead;

        do
        {
            bytesRead = await receivePipe.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            memoryStream.Write(buffer, 0, bytesRead);
        } while (!receivePipe.IsMessageComplete);

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
    
    public async Task WriteStringAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!sendPipe.IsConnected)
        {
            await sendPipe.WaitForConnectionAsync(cancellationToken);
        }

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await sendPipe.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
        await sendPipe.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        receivePipe.Dispose();
        sendPipe.Dispose();
    }
}
