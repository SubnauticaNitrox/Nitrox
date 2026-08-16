using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Channels;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Nitrox.Model.Constants;
using Nitrox.Model.MagicOnion;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Models.Logging.ZLogger;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Connects to a locally running app that might want to track this server. Nitrox.Launcher is expected.
/// </summary>
internal sealed class ServersManagementService(PlayerManager playerManager, IPacketSender packetSender, CommandService commandProcessor, IProgressReporter progressReporter, IOptions<ServerStartOptions> options, ILogger<ServersManagementService> logger) : BackgroundService
{
    public static readonly Channel<LogEntry> LogQueue = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });

    private readonly CommandService commandProcessor = commandProcessor;
    private readonly ILogger<ServersManagementService> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly PlayerManager playerManager = playerManager;
    private readonly IProgressReporter progressReporter = progressReporter;
    private GrpcChannel? channel;
    private ConnectionInfo? currentConnectionInfo;
    private Task? pushLogsTask;
    private Task? pushProgressTask;

    public override void Dispose() => channel?.Dispose();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ServerManagementReceiver? receiver = new(commandProcessor, packetSender);
        IServersManagement api = null;

        using PeriodicTimer refreshTimer = new(TimeSpan.FromSeconds(5));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // On Windows, connection info is the named pipe name
                // On Non-Windows, connection info is the port number
                ConnectionInfo connectionInfo = await GetLauncherConnectionInfoAsync();
                if (!connectionInfo.IsValid)
                {
                    await WaitNextAsync();
                    continue;
                }
                await TryRefreshConnectionAsync(connectionInfo);

                // Push data
                if (api != null)
                {
                    await PushPollDataAsync(api);
                    if (!pushLogsTask.IsBusyOrSuccessful())
                    {
                        pushLogsTask = CreateLoopingTask(PushLogsAsync, api, stoppingToken);
                    }
                    if (!pushProgressTask.IsBusyOrSuccessful())
                    {
                        pushProgressTask = CreateLoopingTask(PushProgressAsync, api, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ShouldIgnoreException(ex))
                {
                    logger.ZLogTrace($"{ex.Message}");
                }
            }
            await WaitNextAsync();
        }

        ValueTask<bool> WaitNextAsync() => refreshTimer.WaitForNextTickAsync(stoppingToken);

        async Task TryRefreshConnectionAsync(ConnectionInfo connectionInfo)
        {
            if (!connectionInfo.IsValid)
            {
                return;
            }
            if (currentConnectionInfo != connectionInfo)
            {
                channel?.Dispose();
                channel = null;
                if (api != null)
                {
                    await api.DisposeAsync();
                }
                api = null;
                currentConnectionInfo = connectionInfo;
            }

            if (OperatingSystem.IsWindows())
            {
                // On Windows, use named pipes for faster IPC
                channel ??= GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
                {
                    HttpHandler = new SocketsHttpHandler
                    {
                        ConnectCallback = async (_, cancellationToken) =>
                        {
                            if (!OperatingSystem.IsWindows())
                            {
                                throw new InvalidOperationException($"{nameof(NamedPipeClientStream)} requires Windows OS");
                            }
                            NamedPipeClientStream? clientStream = new(".", connectionInfo.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                            await clientStream.ConnectAsync(cancellationToken);
                            return clientStream;
                        }
                    }
                });
            }
            else
            {
                channel ??= GrpcChannel.ForAddress($"http://localhost:{connectionInfo.Port}");
            }

            if (api == null)
            {
                StreamingHubClientOptions grpcOptions = StreamingHubClientOptions.CreateWithDefault()
                                                                                 .WithCallOptions(new CallOptions(new Metadata
                                                                                 {
                                                                                     { "ProcessId", Environment.ProcessId.ToString() },
                                                                                     { "SaveName", options.Value.SaveName }
                                                                                 }));
                api = await StreamingHubClient.ConnectAsync<IServersManagement, IServerManagementReceiver>(channel,
                                                                                                           receiver,
                                                                                                           cancellationToken: stoppingToken,
                                                                                                           options: grpcOptions);
            }
        }

        Task CreateLoopingTask(Func<IServersManagement, CancellationToken, Task> action, IServersManagement service, CancellationToken cancellationToken) =>
            Task.Run(async () =>
            {
                try
                {
                    await action(service, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!ShouldIgnoreException(ex))
                    {
                        logger.ZLogTrace(ex, $"Error during looping task");
                    }
                }
            }, cancellationToken);
    }

    private async Task PushPollDataAsync(IServersManagement api)
    {
        await api.SetPlayers(playerManager.ConnectedPlayers().Select(player => player.Name).ToArray());
    }

    private async Task PushLogsAsync(IServersManagement api, CancellationToken cancellationToken)
    {
        await foreach (LogEntry log in LogQueue.Reader.ReadAllAsync(cancellationToken))
        {
            string category = log.Entry.LogInfo.Category.ToString();
            DateTimeOffset time = log.Entry.LogInfo.Timestamp.Local;
            int level = log.Entry.LogInfo.LogLevel switch
            {
                LogLevel.Information => 0,
                LogLevel.Debug => 1,
                LogLevel.Warning => 2,
                LogLevel.Error => 3,
                _ => 0
            };
            bool isPlain = log.Entry.TryGetProperty(out PlainScope _);
            string? message = log.Generator(log.Entry, log.Formatter, log.Writer); // Generator will dispose of the log data, so this needs to be called "last".
            if (message is "")
            {
                continue;
            }
            // Omit last "new line" occurrence, as it is implied.
            if (message.LastIndexOf(Environment.NewLine, StringComparison.Ordinal) is var newlineIndex and > -1)
            {
                message = message.Substring(0, newlineIndex);
            }

            await api.AddOutputLine(category, isPlain ? null : time, level, message);
        }
    }

    private async Task PushProgressAsync(IServersManagement api, CancellationToken cancellationToken)
    {
        await foreach (IProgressReporter.ProgressUpdate progress in progressReporter.ReadAllAsync(cancellationToken))
        {
            await api.SetLoadingProgress(progress.Stage, progress.Progress);
        }
    }

    private bool ShouldIgnoreException(Exception ex)
    {
        ex = ex is AggregateException aggregate ? aggregate.InnerException : ex;
        return ex switch
        {
            RpcException { Status.StatusCode: StatusCode.Unavailable or StatusCode.Cancelled } => true,
            OperationCanceledException => true,
            ObjectDisposedException { ObjectName: nameof(StreamingHubClient) } => true,
            _ => false
        };
    }

    private async Task<ConnectionInfo> GetLauncherConnectionInfoAsync()
    {
        try
        {
            string info = await File.ReadAllTextAsync(Path.Combine(Path.GetTempPath(), LauncherConstants.GRPC_LISTEN_PORT_TEMP_FILE_NAME));
            return ConnectionInfo.From(info);
        }
        catch (Exception)
        {
            logger.ZLogWarningOnce($"Unable to get gRPC connection info from Nitrox Launcher, it might not be running. Retrying...");
        }
        return new ConnectionInfo();
    }

    private class ServerManagementReceiver(CommandService commandProcessor, IPacketSender packetSender) : IServerManagementReceiver
    {
        public void OnCommand(string command) => commandProcessor.ExecuteCommand(command, new HostToServerCommandContext(packetSender), out _);
    }

    internal record LogEntry(IZLoggerEntry Entry, IZLoggerFormatter Formatter, ZLoggerPlainOptions.LogGeneratorCall Generator, ArrayBufferWriter<byte> Writer);

    internal record ConnectionInfo(int Port = 0,
                                   [property: SupportedOSPlatform("windows")]
                                   string? PipeName = null)
    {
        [MemberNotNullWhen(true, nameof(PipeName))]
        public bool IsValid
        {
            get
            {
                if (OperatingSystem.IsWindows())
                {
                    return !string.IsNullOrWhiteSpace(PipeName);
                }
                return Port > 0;
            }
        }

        public static ConnectionInfo From(string info)
        {
            if (string.IsNullOrWhiteSpace(info))
            {
                return new ConnectionInfo();
            }
            info = info.Trim();
            if (int.TryParse(info, out int port))
            {
                return new ConnectionInfo(port);
            }
            return new ConnectionInfo(PipeName: info);
        }
    }
}
