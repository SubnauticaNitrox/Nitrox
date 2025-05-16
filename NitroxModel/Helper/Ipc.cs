#if NET5_0_OR_GREATER
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
    private readonly NamedPipeClientStream sendPipe;
    private readonly CancellationTokenSource cancellationTokenSource;
    
    private Ipc(NamedPipeServerStream receivePipe, NamedPipeClientStream sendPipe, CancellationTokenSource cancellationTokenSource)
    {
        this.receivePipe = receivePipe;
        this.sendPipe = sendPipe;
        this.cancellationTokenSource = cancellationTokenSource;
    }
    
    public static Ipc Create(string pipeName)
    {
        var receivePipe = new NamedPipeServerStream($"{pipeName}_receive", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        var sendPipe = new NamedPipeClientStream(".", $"{pipeName}_send", PipeDirection.Out, PipeOptions.Asynchronous);
        var cancellationTokenSource = new CancellationTokenSource();

        return new Ipc(receivePipe, sendPipe, cancellationTokenSource);
    }

    public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
    {
        if (!receivePipe.IsConnected)
        {
            await receivePipe.WaitForConnectionAsync(cancellationToken);
        }

        using var memoryStream = new MemoryStream();
        var buffer = new byte[1024];
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
            await sendPipe.ConnectAsync(cancellationToken);
        }

        var messageBytes = Encoding.UTF8.GetBytes(message);
        await sendPipe.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
        await sendPipe.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        receivePipe?.Dispose();
        sendPipe?.Dispose();
    }
}
#endif
