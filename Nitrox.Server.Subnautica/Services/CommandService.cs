using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Enables processing of commands from a string.
/// </summary>
internal sealed partial class CommandService(CommandRegistry registry, ILogger<CommandService> logger, ILoggerFactory loggerFactory) : IHostedLifecycleService, ICommandSubmit
{
    private const int MAX_ARGS = 8;
    private readonly ILogger<CommandService> logger = logger;
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    private readonly CommandRegistry registry = registry;
    private readonly Channel<Task> runningCommands = Channel.CreateUnbounded<Task>();
    private Task commandWaiterTask;

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

        // Extract command name from command input.
        int endOfNameIndex = inputText.IndexOf(' ');
        if (endOfNameIndex == -1)
        {
            endOfNameIndex = inputText.Length;
        }
        ReadOnlySpan<char> commandName = inputText[.. endOfNameIndex];

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
                logger.ZLogError($"Too many arguments passed to command {commandName.ToString():@CommandName}");
                return;
            }
            ranges[rangeIndex++] = new Range(match.Index, match.Index + match.Length);
        }

        // Convert text ranges into object args. First arg is always the context (and should be accounted for in algorithm below).
        Span<object> args = new(new object[MAX_ARGS]) { [0] = context };
        CommandHandlerEntry? handler = null;
        bool inputHasCorrectParameterCount = false;
        List<CommandHandlerEntry>? almostMatchingHandlers = null;
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
                object? parsedValue = registry.TryParseToType(part, currentHandler.ParameterTypes[i]);
                if (parsedValue == null)
                {
                    almostMatchingHandlers ??= [];
                    almostMatchingHandlers.Add(currentHandler);

                    // Unset args array except for first arg which is the context.
                    for (int j = i + 1; j >= 0; j--)
                    {
                        args[j] = null;
                    }
                    goto nextHandler;
                }
                args[i + 1] = parsedValue;
            }
            handler = currentHandler;
            break;

            nextHandler: ;
        }
        if (!inputHasCorrectParameterCount)
        {
            logger.ZLogInformation($"Command {commandName.ToString():@CommandName} does not support the provided arguments. See below for more information.");
            ExecuteCommand($"help {commandName}", context);
            return;
        }
        if (handler == null)
        {
            if (almostMatchingHandlers.Count == 0)
            {
                logger.ZLogInformation($"Command {commandName.ToString():@CommandName} failed");
                return;
            }
            QueueTryRunFirstArgConvertedHandler(context, almostMatchingHandlers, commandArgs.ToString(), ranges);
            return;
        }

        RunHandler(handler, args[.. (handler.Parameters.Length + 1)], inputText.ToString());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        commandWaiterTask = Task.Factory.StartNew(() => EnsureCommandsAreProcessedAsync(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.ZLogTrace($"Waiting for commands to finish processing...");
        await commandWaiterTask;
        logger.ZLogTrace($"Done waiting for commands");
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.ZLogInformation($"To get help for commands, run help in console or /help in chatbox");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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

    private void QueueTryRunFirstArgConvertedHandler(ICommandContext context, List<CommandHandlerEntry> looselyCompatibleHandlers, string argsInput, ReadOnlySpan<Range> argRanges)
    {
        List<string> args = [];
        foreach (Range range in argRanges)
        {
            if (range is { Start.Value: 0, End.Value: 0 })
            {
                continue;
            }
            args.Add(argsInput[range]);
        }

        Task tryRunHandlerTask = Task.Run(async () =>
        {
            List<(CommandHandlerEntry handler, ConvertResult[][] conversions)> failedHandlers = [];
            foreach (CommandHandlerEntry handler in looselyCompatibleHandlers)
            {
                ConvertResult[][] values = await TryConvertStringParamsToObjectParams(registry, args, handler.ParameterTypes);
                if (values.Any(v => v.LastOrDefault().Success != true))
                {
                    failedHandlers.Add((handler, values));
                    continue;
                }

                RunHandler(handler, [context, ..values.Select(v => v.LastOrDefault().Value).ToArray()], argsInput);
                return;
            }

            logger.ZLogInformation($"Command '{$"{looselyCompatibleHandlers[0].Name} {argsInput}":@Command}' failed to match to any command handlers.{Environment.NewLine}{GetErrorMessagesFromFailedHandlers(failedHandlers)}");
        }).ContinueWithHandleError(exception => logger.ZLogError(exception, $"Error while parsing '{argsInput}' to a command handler"));
        runningCommands.Writer.TryWrite(tryRunHandlerTask);

        static async Task<ConvertResult[][]> TryConvertStringParamsToObjectParams(CommandRegistry registry, List<string> args, Type[] parameterTypes)
        {
            List<ConvertResult[]> result = null;
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                ConvertResult[] conversions;
                if (registry.TryParseToType(args[i], parameterTypes[i]) is { } parsedValue)
                {
                    conversions = [ConvertResult.Ok(parsedValue)];
                }
                else
                {
                    conversions = [ConvertResult.Fail($"Failed to parse {args[i]} to a {parameterTypes[i].Name}")];
                }
                if (conversions is [] || !conversions[0].Success)
                {
                    conversions = [..conversions, ..await registry.TryConvertToType(args[i], parameterTypes[i])];
                }

                result ??= [];
                result.Add(conversions);
            }
            return result?.ToArray() ?? [];
        }

        static string GetErrorMessagesFromFailedHandlers(List<(CommandHandlerEntry handler, ConvertResult[][] conversions)> failedHandlers)
        {
            StringBuilder sb = new();
            int indent = 0;
            foreach ((CommandHandlerEntry handler, ConvertResult[][] conversions) handlerResult in failedHandlers)
            {
                for (int argIndex = 0; argIndex < handlerResult.conversions.Length; argIndex++)
                {
                    sb.Append("Arg ")
                      .Append(argIndex.ToString())
                      .AppendLine(": ");
                    indent++;
                    string[] messages = handlerResult.conversions[argIndex].Where(c => c is { Success: false, Value: string }).Select(c => c.Value).OfType<string>().ToArray();
                    for (int i = 0; i < messages.Length; i++)
                    {
                        sb.Append(GetIndentText()).Append("- ").Append(messages[i]);
                        if (i != messages.Length - 1)
                        {
                            sb.AppendLine();
                        }
                    }
                    indent--;
                    sb.AppendLine();
                }
            }
            return sb.ToString();

            string GetIndentText() => new(' ', indent * 4);
        }
    }

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
                logger.ZLogError($"Failed to track command task");
            }
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Error occurred while executing command {inputText:@Command}");
        }
    }
}
