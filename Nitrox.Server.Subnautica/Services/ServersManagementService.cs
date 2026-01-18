using System.Buffers;
using System.IO;
using System.Threading.Channels;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Nitrox.Model.Constants;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.MagicOnion;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Models.Logging.ZLogger;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Connects to a locally running app that might want to track this server. Nitrox.Launcher is expected.
/// </summary>
internal sealed class ServersManagementService(PlayerManager playerManager, IPacketSender packetSender, CommandService commandProcessor, IOptions<ServerStartOptions> options, ILogger<ServersManagementService> logger) : BackgroundService
{
    public static readonly Channel<LogEntry> LogQueue = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });
    private readonly CommandService commandProcessor = commandProcessor;
    private readonly ILogger<ServersManagementService> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly PlayerManager playerManager = playerManager;
    private GrpcChannel? channel;
    private Task? pushLogsTask;

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
                int? launcherGrpcPortAsync = await GetLauncherGrpcPortAsync();
                if (launcherGrpcPortAsync == null)
                {
                    await WaitNextAsync();
                    continue;
                }
                await RefreshConnectionAsync(launcherGrpcPortAsync);

                // Push data
                await PushPollDataAsync(api);
                if (!pushLogsTask.IsBusyOrDone())
                {
                    pushLogsTask = CreateLoopingTask(PushLogsAsync, api, stoppingToken);
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

        async Task RefreshConnectionAsync(int? grpcPort)
        {
            if (channel?.Target.EndsWith(grpcPort.ToString(), StringComparison.Ordinal) == false)
            {
                channel?.Dispose();
                channel = null;
                if (api != null)
                {
                    await api.DisposeAsync();
                }
                api = null;
            }

            channel ??= GrpcChannel.ForAddress($"http://localhost:{grpcPort}");
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
                        logger.ZLogError(ex, $"Error during looping task");
                    }
                }
            }, cancellationToken);
    }

    private async Task PushPollDataAsync(IServersManagement api)
    {
        await api.SetPlayerCount(playerManager.PlayerCount);
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

    private bool ShouldIgnoreException(Exception ex)
    {
        ex = ex is AggregateException aggregate ? aggregate.InnerException : ex;
        return ex switch
        {
            RpcException { Status.StatusCode: StatusCode.Unavailable or StatusCode.Cancelled } => true,
            OperationCanceledException => true,
            _ => false
        };
    }

    private async Task<int?> GetLauncherGrpcPortAsync()
    {
        try
        {
            string port = await File.ReadAllTextAsync(Path.Combine(Path.GetTempPath(), LauncherConstants.GRPC_LISTEN_PORT_TEMP_FILE_NAME));
            if (int.TryParse(port.Trim(), out int result))
            {
                return result;
            }
        }
        catch (Exception)
        {
            logger.ZLogWarningOnce($"Unable to get gRPC listen port from Nitrox Launcher, it might not be running. Retrying...");
        }
        return null;
    }

    private class ServerManagementReceiver(CommandService commandProcessor, IPacketSender packetSender) : IServerManagementReceiver
    {
        public void OnCommand(string command) => commandProcessor.ExecuteCommand(command, new HostToServerCommandContext(packetSender));
    }

    internal record LogEntry(IZLoggerEntry Entry, IZLoggerFormatter Formatter, ZLoggerPlainOptions.LogGeneratorCall Generator, ArrayBufferWriter<byte> Writer);
}
