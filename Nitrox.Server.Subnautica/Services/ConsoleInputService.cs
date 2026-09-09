using System.Text;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Reads console input and handles input history.
/// </summary>
internal sealed class ConsoleInputService(CommandService commandService, IPacketSender packetSender, IHostApplicationLifetime appLifetime, ILogger<ConsoleInputService> logger) : BackgroundService
{
    private readonly IHostApplicationLifetime appLifetime = appLifetime;
    private readonly CommandService commandService = commandService;
    private readonly CircularBuffer<string> inputHistory = new(1000);
    private readonly ILogger<ConsoleInputService> logger = logger;
    private readonly IPacketSender packetSender = packetSender;
    private int currentHistoryIndex;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (Console.IsInputRedirected)
            {
                return;
            }

            Console.TreatControlCAsInput = true;
            await HandleInputAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Failed to handle console input");
        }
    }

    private void SubmitInput(string input) => commandService.ExecuteCommand(input, new HostToServerCommandContext(packetSender), out _);

    private async Task HandleInputAsync(CancellationToken cancellationToken)
    {
        StringBuilder inputLineBuilder = new();
        // Tracks the caret position ourselves instead of reading it back from Console.CursorLeft: on Unix, that getter
        // round-trips an ANSI cursor-position query through the same stdin stream that key presses are read from, which
        // some non-native terminal front-ends (e.g. PufferPanel's console) don't relay correctly, corrupting input.
        int caretIndex = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!Console.KeyAvailable)
            {
                try
                {
                    await Task.Delay(10, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // ignored
                }
                continue;
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            // Handle (ctrl) hotkeys
            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.C:
                        if (inputLineBuilder.Length > 0)
                        {
                            ClearInputLine();
                            continue;
                        }

                        appLifetime.StopApplication();
                        return;
                    case ConsoleKey.D:
                        appLifetime.StopApplication();
                        return;
                    default:
                        // Unhandled modifier key
                        continue;
                }
            }

            if (keyInfo.Modifiers == 0)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow when caretIndex > 0:
                        caretIndex--;
                        Console.CursorLeft = caretIndex;
                        continue;
                    case ConsoleKey.RightArrow when caretIndex < inputLineBuilder.Length:
                        caretIndex++;
                        Console.CursorLeft = caretIndex;
                        continue;
                    case ConsoleKey.Backspace:
                        if (inputLineBuilder.Length > caretIndex - 1 && caretIndex > 0)
                        {
                            inputLineBuilder.Remove(caretIndex - 1, 1);
                            caretIndex--;
                            Console.CursorLeft = caretIndex;
                            Console.Write(' ');
                            Console.CursorLeft = caretIndex;
                            RedrawInput();
                        }
                        continue;
                    case ConsoleKey.Delete:
                        if (inputLineBuilder.Length > 0 && caretIndex < inputLineBuilder.Length)
                        {
                            inputLineBuilder.Remove(caretIndex, 1);
                            RedrawInput(caretIndex, inputLineBuilder.Length - caretIndex);
                        }
                        continue;
                    case ConsoleKey.Home:
                        caretIndex = 0;
                        Console.CursorLeft = caretIndex;
                        continue;
                    case ConsoleKey.End:
                        caretIndex = inputLineBuilder.Length;
                        Console.CursorLeft = caretIndex;
                        continue;
                    case ConsoleKey.Escape:
                        ClearInputLine();
                        continue;
                    case ConsoleKey.Tab:
                        if (caretIndex + 4 < GetConsoleWidth())
                        {
                            inputLineBuilder.Insert(caretIndex, "    ");
                            RedrawInput(caretIndex, -1);
                            caretIndex += 4;
                            Console.CursorLeft = caretIndex;
                        }
                        continue;
                    case ConsoleKey.UpArrow when inputHistory.Count > 0 && currentHistoryIndex > -inputHistory.Count:
                        inputLineBuilder.Clear();
                        inputLineBuilder.Append(inputHistory[--currentHistoryIndex]);
                        RedrawInput();
                        caretIndex = Math.Min(inputLineBuilder.Length, GetConsoleWidth());
                        Console.CursorLeft = caretIndex;
                        continue;
                    case ConsoleKey.DownArrow when inputHistory.Count > 0 && currentHistoryIndex < 0:
                        if (currentHistoryIndex == -1)
                        {
                            ClearInputLine();
                            continue;
                        }
                        inputLineBuilder.Clear();
                        inputLineBuilder.Append(inputHistory[++currentHistoryIndex]);
                        RedrawInput();
                        caretIndex = Math.Min(inputLineBuilder.Length, GetConsoleWidth());
                        Console.CursorLeft = caretIndex;
                        continue;
                }
            }
            // Handle input submit to submit handler
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                string submit = inputLineBuilder.ToString();
                if (string.IsNullOrWhiteSpace(submit))
                {
                    inputLineBuilder.Clear();
                    continue;
                }
                if (inputHistory.Count == 0 || inputHistory[inputHistory.LastChangedIndex] != submit)
                {
                    inputHistory.Add(submit);
                }
                currentHistoryIndex = 0;
                inputLineBuilder.Clear();
                caretIndex = 0;
                Console.WriteLine();
                SubmitInput(submit);
                continue;
            }

            // If unhandled key, append as input.
            if (keyInfo.KeyChar != 0)
            {
                Console.Write(keyInfo.KeyChar);
                caretIndex++;
                if (caretIndex - 1 < inputLineBuilder.Length)
                {
                    try
                    {
                        inputLineBuilder.Insert(caretIndex - 1, keyInfo.KeyChar);
                    }
                    catch (Exception ex) when (ex is IndexOutOfRangeException or ArgumentOutOfRangeException)
                    {
                        // ignored
                    }
                    RedrawInput(caretIndex, -1);
                }
                else
                {
                    inputLineBuilder.Append(keyInfo.KeyChar);
                }
            }
        }

        int GetConsoleWidth(int offset = 0) => int.Max(0, Console.WindowWidth + offset);

        void ClearInputLine()
        {
            currentHistoryIndex = 0;
            inputLineBuilder.Clear();
            caretIndex = 0;
            Console.Write($"\r{new string(' ', GetConsoleWidth(-1))}\r");
        }

        void RedrawInput(int start = 0, int end = 0)
        {
            int lastPosition = caretIndex;
            // Expand range to end if end value is -1
            if (start > -1 && end == -1)
            {
                end = int.Max(inputLineBuilder.Length - start, 0);
            }

            if (start == 0 && end == 0)
            {
                // Redraw entire line
                Console.Write($"\r{new string(' ', GetConsoleWidth(-1))}\r{inputLineBuilder}");
            }
            else if (start > -1)
            {
                // Redraw part of line
                start = int.Min(start, inputLineBuilder.Length);
                string changedInputSegment = inputLineBuilder.ToString(start, end);
                Console.CursorVisible = false;
                Console.Write($"{changedInputSegment}{new string(' ', int.Max(0, inputLineBuilder.Length - changedInputSegment.Length + 1))}");
                Console.CursorVisible = true;
            }
            Console.CursorLeft = lastPosition;
        }
    }
}
