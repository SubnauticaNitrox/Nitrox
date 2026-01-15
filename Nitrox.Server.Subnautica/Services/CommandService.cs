extern alias JB;
using System.Threading.Channels;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Processor;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Enables processing of commands from a string.
/// </summary>
internal sealed class CommandService(TextCommandProcessor textCommandProcessor, ILogger<CommandService> logger) : IHostedService
{
    private readonly TextCommandProcessor textCommandProcessor = textCommandProcessor;
    private readonly Channel<Task> runningCommands = Channel.CreateUnbounded<Task>();
    private Task? commandWaiterTask;

    public void ExecuteCommand(string command)
    {
        // TODO: Make ProcessCommand async
        runningCommands.Writer.TryWrite(Task.Run(() => textCommandProcessor.ProcessCommand(command, Optional.Empty, Perms.HOST)));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        commandWaiterTask = Task.Factory.StartNew(async () =>
        {
            await foreach (Task task in runningCommands.Reader.ReadAllAsync(cancellationToken))
            {
                if (task == null)
                {
                    continue;
                }
                await task;
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (commandWaiterTask == null)
        {
            return;
        }
        if (commandWaiterTask.IsCompletedSuccessfully)
        {
            return;
        }

        logger.ZLogTrace($"Waiting for commands to finish processing...");
        await commandWaiterTask;
        logger.ZLogTrace($"Done waiting for commands");
    }
}
