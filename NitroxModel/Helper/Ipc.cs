#if NET5_0_OR_GREATER
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroxModel.Helper;

public static class Ipc
{
    private static string PipeName(int processId) => $"NitroxServer_{processId}";
    
    // Working: Receiving commands from client | WIP: Sending server output (plus other data like server name, player count, online status, etc) to client
    public class ServerIpc : IDisposable
    {
        public readonly int ProcessId;
        private readonly NamedPipeServerStream serverPipe;
        private readonly CancellationTokenSource cancellationTokenSource;
    
        public ServerIpc(int processId, CancellationTokenSource cancellationTokenSource)
        {
            ProcessId = processId;
            this.cancellationTokenSource = cancellationTokenSource;
            
            serverPipe = new NamedPipeServerStream(PipeName(ProcessId), PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        }
        
        // WIP
        public async Task<bool> SendOutput(string output, CancellationToken cancellationToken = default)
        {
            if (!serverPipe.IsConnected)
            {
                await serverPipe.WaitForConnectionAsync(cancellationToken);
            }
            byte[] outputBytes = Encoding.UTF8.GetBytes(output);
            await serverPipe.WriteAsync(BitConverter.GetBytes((uint)outputBytes.Length), cancellationToken);
            await serverPipe.WriteAsync(outputBytes, cancellationToken);
            await serverPipe.FlushAsync(cancellationToken);
            return true;
        }
        
        public static ServerIpc StartReadingCommands(int processId, Action<string> onCommandReceived, CancellationToken cancellationToken = default)
        {
            Log.Info("Starting Server IPC");
            ArgumentNullException.ThrowIfNull(onCommandReceived);

            ServerIpc ipc = new(processId, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
            Thread thread = new(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        string command = await ipc.ReadStringAsync(cancellationToken);
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
            return ipc;
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
    
        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            serverPipe.Dispose();
        }
    }

    // Working: Sending commands to server | WIP: Receiving output from server (currently being done the old way within ServerEntry.cs)
    public class ClientIpc : IDisposable
    {
        public readonly int ProcessId;
        private readonly NamedPipeClientStream clientPipe;
        private readonly CancellationTokenSource cancellationTokenSource;
    
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
        
        // WIP
        public void StartReadingServerOutput(Action<string> onServerOutputReceived, CancellationToken cancellationToken = default)
        {
            Log.Info($"Starting client IPC \"{PipeName(ProcessId)}\"");
            ArgumentNullException.ThrowIfNull(onServerOutputReceived);
            
            Thread thread = new(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        string output = await ReadStringAsync(cancellationToken);
                        onServerOutputReceived(output);
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
        
        // WIP
        public async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            if (!await WaitForConnection())
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
        
        private async Task<bool> WaitForConnection()
        {
            if (clientPipe.IsConnected)
            {
                return true;
            }
            try
            {
                await clientPipe.ConnectAsync();
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
    
        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            clientPipe.Dispose();
        }
    }
}
#endif
