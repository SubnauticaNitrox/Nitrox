#if NET5_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroxModel.Helper;

public static class Ipc
{
    private static string PipeName(int processId) => $"NitroxServer_{processId}";

    public static class Messages
    {
        public static string StopMessage => "__SERVER_STOPPED__";
        public static string SaveNameMessage => "__SAVE_NAME__";
        public static string PlayerCountMessage => "__PLAYER_COUNT__";

        public static List<string> AllMessages { get; } = [StopMessage, SaveNameMessage, PlayerCountMessage];
    }

    public sealed class ServerIpc : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ConcurrentQueue<(string Output, CancellationToken Token)> outputBuffer = new();
        private readonly SemaphoreSlim outputSemaphore = new(1, 1);
        public readonly int ProcessId;
        private readonly NamedPipeServerStream serverPipe;
        private bool isProcessingBuffer;

        public ServerIpc(int processId, CancellationTokenSource cancellationTokenSource)
        {
            ProcessId = processId;
            this.cancellationTokenSource = cancellationTokenSource;

            Log.Info($"Creating IPC for process {processId}.");
            serverPipe = new NamedPipeServerStream(PipeName(ProcessId), PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public Task<bool> SendOutput(string output, CancellationToken cancellationToken = default)
        {
            outputBuffer.Enqueue((output, cancellationToken));
            ProcessBuffer();
            return Task.FromResult(true);
        }

        public void StartReadingCommands(Action<string> onCommandReceived, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(onCommandReceived);

            Thread thread = new(async void () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        string command = await ReadStringAsync(cancellationToken);
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
        }

        public void Dispose()
        {
            try
            {
                if (serverPipe.IsConnected)
                {
                    byte[] stopMsg = Encoding.UTF8.GetBytes(Messages.StopMessage);
                    serverPipe.Write(BitConverter.GetBytes((uint)stopMsg.Length), 0, 4);
                    serverPipe.Write(stopMsg, 0, stopMsg.Length);
                    serverPipe.Flush();
                }
            }
            catch
            {
                // ignore
            }
            cancellationTokenSource.Cancel();
            serverPipe.Dispose();
        }

        private async void ProcessBuffer()
        {
            if (isProcessingBuffer)
            {
                return;
            }
            isProcessingBuffer = true;
            await outputSemaphore.WaitAsync();
            try
            {
                while (outputBuffer.TryDequeue(out (string Output, CancellationToken Token) item))
                {
                    if (!serverPipe.IsConnected)
                    {
                        await serverPipe.WaitForConnectionAsync(item.Token);
                    }
                    byte[] outputBytes = Encoding.UTF8.GetBytes(item.Output);
                    await serverPipe.WriteAsync(BitConverter.GetBytes((uint)outputBytes.Length), item.Token);
                    await serverPipe.WriteAsync(outputBytes, item.Token);
                    await serverPipe.FlushAsync(item.Token);
                }
            }
            finally
            {
                isProcessingBuffer = false;
                outputSemaphore.Release();
            }
        }

        private async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            if (!await WaitForConnection())
            {
                return "";
            }

            try
            {
                byte[] sizeBytes = new byte[4];
                await serverPipe.ReadExactlyAsync(sizeBytes, cancellationToken);
                byte[] stringBytes = new byte[BitConverter.ToUInt32(sizeBytes)];
                await serverPipe.ReadExactlyAsync(stringBytes, cancellationToken);

                return Encoding.UTF8.GetString(stringBytes);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private async Task<bool> WaitForConnection()
        {
            if (serverPipe.IsConnected)
            {
                return true;
            }
            try
            {
                await serverPipe.WaitForConnectionAsync();
                return true;
            }
            catch (IOException)
            {
                try
                {
                    serverPipe.Disconnect();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return false;
        }
    }

    public sealed class ClientIpc : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly NamedPipeClientStream clientPipe;
        public readonly int ProcessId;

        public ClientIpc(int processId, CancellationTokenSource cancellationTokenSource)
        {
            ProcessId = processId;
            this.cancellationTokenSource = cancellationTokenSource;

            clientPipe = new NamedPipeClientStream(".", PipeName(ProcessId), PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public async Task<bool> SendCommand(string command, CancellationToken cancellationToken = default)
        {
            if (!clientPipe.IsConnected)
            {
                await clientPipe.ConnectAsync(1000, cancellationToken);
            }
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            await clientPipe.WriteAsync(BitConverter.GetBytes((uint)commandBytes.Length), cancellationToken);
            await clientPipe.WriteAsync(commandBytes, cancellationToken);
            await clientPipe.FlushAsync(cancellationToken);
            return true;
        }

        public void StartReadingServerOutput(Action<string> onServerOutputReceived, Action? onServerStopped = null, CancellationToken cancellationToken = default)
        {
            Log.Info($"Starting client IPC \"{PipeName(ProcessId)}\"");
            ArgumentNullException.ThrowIfNull(onServerOutputReceived);

            Thread thread = new(async void () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        string output = await ReadStringAsync(cancellationToken);
                        if (output == Messages.StopMessage)
                        {
                            onServerStopped?.Invoke();
                            break;
                        }
                        if (string.IsNullOrEmpty(output))
                        {
                            continue;
                        }
                        onServerOutputReceived(output);
                    }
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
                catch (TimeoutException)
                {
                    // Connection timed out
                    onServerStopped?.Invoke();
                }
                catch (IOException)
                {
                    // Pipe closed unexpectedly (server killed or crashed)
                    onServerStopped?.Invoke();
                }
                catch (ObjectDisposedException)
                {
                    // Pipe disposed (server killed or crashed)
                    onServerStopped?.Invoke();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            if (!await WaitForConnection(cancellationToken))
            {
                return "";
            }

            try
            {
                byte[] sizeBytes = new byte[4];
                await clientPipe.ReadExactlyAsync(sizeBytes, cancellationToken);
                byte[] stringBytes = new byte[BitConverter.ToUInt32(sizeBytes)];
                await clientPipe.ReadExactlyAsync(stringBytes, cancellationToken);

                return Encoding.UTF8.GetString(stringBytes);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            clientPipe.Dispose();
        }

        private async Task<bool> WaitForConnection(CancellationToken cancellationToken = default)
        {
            if (clientPipe.IsConnected)
            {
                return true;
            }
            try
            {
                await clientPipe.ConnectAsync(1000, cancellationToken);
                return true;
            }
            catch (IOException)
            {
                try
                {
                    clientPipe.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return false;
        }
    }
}
#endif
