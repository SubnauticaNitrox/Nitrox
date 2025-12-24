using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Nitrox.Model.Constants;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.MagicOnion;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Connects to locally running app that might want to track this server. Nitrox.Launcher is expected.
/// </summary>
internal sealed partial class ServersManagementService(PlayerManager playerManager, TextCommandProcessor commandProcessor, IOptions<ServerStartOptions> options, ILogger<ServersManagementService> logger) : BackgroundService
{
    public static readonly Channel<string> LogQueue = Channel.CreateBounded<string>(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropOldest });
    private readonly TextCommandProcessor commandProcessor = commandProcessor;
    private readonly ILogger<ServersManagementService> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly PlayerManager playerManager = playerManager;
    private GrpcChannel? channel;
    private Task? pushLogsTask;

    [GeneratedRegex(@"(?:\[(?<timestamp>\d{2}:\d{2}:\d{2}\.\d{3})\]\s\[(?<level>\w+)\]\s(?<category>\w+):\s)?(?<logText>(?:.|\n)*?(?=$|\n\[))")]
    private static partial Regex LogRegex { get; }

    public override void Dispose() => channel?.Dispose();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ServerManagementReceiver? receiver = new(commandProcessor);
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
                // Init gRPC channel or update
                if (channel == null || api == null || !channel.Target.EndsWith(launcherGrpcPortAsync.ToString(), StringComparison.Ordinal))
                {
                    channel?.Dispose();
                    channel = GrpcChannel.ForAddress($"http://localhost:{launcherGrpcPortAsync}");
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
        await foreach (string log in LogQueue.Reader.ReadAllAsync(cancellationToken))
        {
            if (LogRegex.Match(log) is not { Success: true } match)
            {
                continue;
            }

            int level = match.Groups["level"].Value switch
            {
                "info" => 0,
                "dbug" => 1,
                "warn" => 2,
                "fail" => 3,
                _ => 0
            };
            await api.AddOutputLine(match.Groups["category"].Value, match.Groups["timestamp"].Value, level, match.Groups["logText"].Value);
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

    private class ServerManagementReceiver(TextCommandProcessor commandProcessor) : IServerManagementReceiver
    {
        public void OnCommand(string command) => commandProcessor.ProcessCommand(command, Optional.Empty, Perms.HOST);
    }
}
