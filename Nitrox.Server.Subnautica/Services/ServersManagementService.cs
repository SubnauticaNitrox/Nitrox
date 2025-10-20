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
internal sealed partial class ServersManagementService(PlayerManager playerManager, ConsoleCommandProcessor commandProcessor, IOptions<ServerStartOptions> options, ILogger<ServersManagementService> logger) : BackgroundService
{
    public static readonly Channel<string> LogQueue = Channel.CreateUnbounded<string>();
    private readonly ConsoleCommandProcessor commandProcessor = commandProcessor;
    private readonly ILogger<ServersManagementService> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly PlayerManager playerManager = playerManager;
    private GrpcChannel? channel;
    private Task? pushLogsTask;

    [GeneratedRegex(@"\[(?<timestamp>\d{2}:\d{2}:\d{2}\.\d{3})\]\s\[(?<level>\w+)\]\s(?<category>\w+):\s(?<logText>(?:.|\n)*?(?=$|\n\[))")]
    private static partial Regex LogRegex { get; }

    public override void Dispose() => channel?.Dispose();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        channel = GrpcChannel.ForAddress($"http://localhost:{LauncherConstants.LAUNCHER_SERVER_TRACK_PORT}");
        ServerManagementReceiver? receiver = new(commandProcessor, options);
        IServersManagement api = null;

        using PeriodicTimer refreshTimer = new(TimeSpan.FromSeconds(5));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                api ??= await StreamingHubClient.ConnectAsync<IServersManagement, IServerManagementReceiver>(channel, receiver, cancellationToken: stoppingToken);
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
            await refreshTimer.WaitForNextTickAsync(stoppingToken);
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
                        logger.ZLogError(ex, $"Error trying to push logs");
                    }
                }
            }, cancellationToken);
    }

    private async Task PushPollDataAsync(IServersManagement api)
    {
        await api.SetPlayerCount(Environment.ProcessId, playerManager.PlayerCount);
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
                _ => 1
            };
            await api.AddOutputLine(Environment.ProcessId, match.Groups["category"].Value, match.Groups["timestamp"].Value, level, match.Groups["logText"].Value);
        }
    }

    private bool ShouldIgnoreException(Exception ex)
    {
        ex = ex is AggregateException aggregate ? aggregate.InnerException : ex;
        if (ex is RpcException { Status.StatusCode: StatusCode.Unavailable or StatusCode.Cancelled })
        {
            return true;
        }
        if (ex is OperationCanceledException)
        {
            return true;
        }
        return false;
    }

    private class ServerManagementReceiver(ConsoleCommandProcessor commandProcessor, IOptions<ServerStartOptions> options) : IServerManagementReceiver
    {
        public Task<string> OnRequestSaveName() => Task.FromResult(options.Value.SaveName);
        public Task<int> OnRequestProcessId() => Task.FromResult(Environment.ProcessId);

        public void OnCommand(string command) => commandProcessor.ProcessCommand(command, Optional.Empty, Perms.HOST);
    }
}
