extern alias JB;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Enables processing of commands from a string.
/// </summary>
internal sealed partial class CommandService(CommandRegistry registry, ILogger<CommandService> logger, ILoggerFactory loggerFactory) : IHostedLifecycleService
{
    private const int MAX_ARGS = 8;
    private readonly ILogger<CommandService> logger = logger;
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly Channel<Task> runningCommands = Channel.CreateUnbounded<Task>();
    private Task commandWaiterTask;

    private readonly CommandRegistry registry = registry;

    [GeneratedRegex(@"""(?:[^""\\]|\\.)*""|\S+", RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture)]
    private static partial Regex ArgumentsRegex { get; }

    /// <summary>
    ///     Tries to execute the command that matches the input.
    /// </summary>
    /// <param name="inputText">The text input which should be interpreted as a command.</param>
    /// <param name="context">The context that should be given to the command handler if found.</param>
    public void ExecuteCommand(ReadOnlySpan<char> inputText, ICommandContext context)
    {
        inputText = inputText.Trim();
        if (inputText.IsEmpty)
        {
            return;
        }

        // TODO: In .NET 10 use new dictionary ignore-case lookup.
        int endOfNameIndex = inputText.IndexOf(' ');
        if (endOfNameIndex == -1)
        {
            endOfNameIndex = inputText.Length;
        }
        Span<char> commandName = endOfNameIndex < 32 ? stackalloc char[endOfNameIndex] : new char[endOfNameIndex];
        inputText[.. endOfNameIndex].ToLowerInvariant(commandName);

        if (!registry.TryGetHandlersByCommandName(context, commandName, out List<CommandHandlerEntry> handlers))
        {
            logger.ZLogInformation($"Unknown command {commandName.ToString():@CommandName}");
            return;
        }

        ReadOnlySpan<char> commandArgs = inputText[endOfNameIndex ..].Trim();

        // Get text ranges within the input that are potentially valid arguments.
        Span<Range> ranges = stackalloc Range[MAX_ARGS];
        Regex.ValueMatchEnumerator argsEnumerator = ArgumentsRegex.EnumerateMatches(commandArgs);
        int rangeIndex = 0;
        while (argsEnumerator.MoveNext())
        {
            ValueMatch match = argsEnumerator.Current;
            if (rangeIndex >= MAX_ARGS)
            {
                logger.LogError("Too many arguments passed to command {CommandName}", commandName.ToString());
                return;
            }
            ranges[rangeIndex++] = new Range(match.Index, match.Index + match.Length);
        }

        // Convert text ranges into object args. First arg is always the context (and should be accounted for in algorithm below).
        Span<object> args = new(new object[MAX_ARGS]) { [0] = context };
        CommandHandlerEntry handler = null;
        bool inputHasCorrectParameterCount = false;
        foreach (CommandHandlerEntry currentHandler in handlers)
        {
            if (currentHandler.Parameters.Length != rangeIndex)
            {
                continue;
            }
            inputHasCorrectParameterCount = true;
            for (int i = 0; i < currentHandler.ParameterTypes.Length; i++)
            {
                ReadOnlySpan<char> part = commandArgs[ranges[i]] switch
                {
                    ['"', .., '"'] s when s.IndexOf("\\\"") >= 0 => s[1..^1].ToString().Replace(@"\""", @""""),
                    ['"', .., '"'] s => s[1..^1],
                    var s => s
                };
                ConvertResult argConversion = registry.TryConvertToType(part, currentHandler.ParameterTypes[i]);
                if (!argConversion.Success)
                {
                    // Unset args array except for first arg which is the context.
                    for (int j = i + 1; j >= 0; j--)
                    {
                        args[j] = null;
                    }
                    goto nextHandler;
                }
                args[i + 1] = argConversion.Value;
            }
            handler = currentHandler;
            break;

            nextHandler: ;
        }
        if (!inputHasCorrectParameterCount)
        {
            logger.LogInformation("Command {CommandName} does not support the provided arguments. See below for more information.", commandName.ToString());
            ExecuteCommand($"help {commandName}", context);
            return;
        }
        if (handler == null)
        {
            logger.LogInformation("Command {CommandName} failed", commandName.ToString());
            return;
        }

        RunHandler(handler, args[.. (handler.Parameters.Length + 1)], inputText.ToString());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        commandWaiterTask = Task.Factory.StartNew(() => EnsureCommandsAreProcessedAsync(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        return Task.CompletedTask;
    }

    private async Task EnsureCommandsAreProcessedAsync(CancellationToken cancellationToken = default)
    {
        await foreach (Task task in runningCommands.Reader.ReadAllAsync(cancellationToken))
        {
            if (task == null)
            {
                continue;
            }
            await task;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Waiting for commands to finish processing...");
        await commandWaiterTask;
        logger.LogDebug("Done waiting for commands");
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("To get help for commands, run help in console or /help in chatbox");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void RunHandler(CommandHandlerEntry handler, Span<object> args, string inputText)
    {
        if (args.Length > 0 && args[0] is ICommandContext context)
        {
            context.Logger = loggerFactory.CreateLogger(handler.Owner.GetType());
        }

        try
        {
            if (!runningCommands.Writer.TryWrite(handler.InvokeAsync(args)))
            {
                logger.LogError("Failed to track command task");
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error occurred while executing command {inputText:@Command}");
        }
    }
}
