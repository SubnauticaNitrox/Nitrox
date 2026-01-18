using System.Runtime.InteropServices;
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

    private void SubmitInput(string input) => commandService.ExecuteCommand(input, new HostToServerCommandContext(packetSender));

    private async Task HandleInputAsync(CancellationToken cancellationToken)
    {
        StringBuilder inputLineBuilder = new();

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
                    case ConsoleKey.LeftArrow when Console.CursorLeft > 0:
                        Console.CursorLeft--;
                        continue;
                    case ConsoleKey.RightArrow when Console.CursorLeft < inputLineBuilder.Length:
                        Console.CursorLeft++;
                        continue;
                    case ConsoleKey.Backspace:
                        if (inputLineBuilder.Length > Console.CursorLeft - 1 && Console.CursorLeft > 0)
                        {
                            inputLineBuilder.Remove(Console.CursorLeft - 1, 1);
                            Console.CursorLeft--;
                            Console.Write(' ');
                            Console.CursorLeft--;
                            RedrawInput();
                        }
                        continue;
                    case ConsoleKey.Delete:
                        if (inputLineBuilder.Length > 0 && Console.CursorLeft < inputLineBuilder.Length)
                        {
                            inputLineBuilder.Remove(Console.CursorLeft, 1);
                            RedrawInput(Console.CursorLeft, inputLineBuilder.Length - Console.CursorLeft);
                        }
                        continue;
                    case ConsoleKey.Home:
                        Console.CursorLeft = 0;
                        continue;
                    case ConsoleKey.End:
                        Console.CursorLeft = inputLineBuilder.Length;
                        continue;
                    case ConsoleKey.Escape:
                        ClearInputLine();
                        continue;
                    case ConsoleKey.Tab:
                        if (Console.CursorLeft + 4 < GetConsoleWidth())
                        {
                            inputLineBuilder.Insert(Console.CursorLeft, "    ");
                            RedrawInput(Console.CursorLeft, -1);
                            Console.CursorLeft += 4;
                        }
                        continue;
                    case ConsoleKey.UpArrow when inputHistory.Count > 0 && currentHistoryIndex > -inputHistory.Count:
                        inputLineBuilder.Clear();
                        inputLineBuilder.Append(inputHistory[--currentHistoryIndex]);
                        RedrawInput();
                        Console.CursorLeft = Math.Min(inputLineBuilder.Length, GetConsoleWidth());
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
                        Console.CursorLeft = Math.Min(inputLineBuilder.Length, GetConsoleWidth());
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
                Console.WriteLine();
                SubmitInput(submit);
                continue;
            }

            // If unhandled key, append as input.
            if (keyInfo.KeyChar != 0)
            {
                Console.Write(keyInfo.KeyChar);
                if (Console.CursorLeft - 1 < inputLineBuilder.Length)
                {
                    try
                    {
                        inputLineBuilder.Insert(Console.CursorLeft - 1, keyInfo.KeyChar);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // ignored
                    }
                    RedrawInput(Console.CursorLeft, -1);
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
            Console.Write($"\r{new string(' ', GetConsoleWidth(-1))}\r");
        }

        void RedrawInput(int start = 0, int end = 0)
        {
            int lastPosition = Console.CursorLeft;
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

    /// <summary>
    ///     Provides a <see cref="IHostLifetime" /> implementation that prevents Ctrl+C from cancelling the
    ///     application.
    /// </summary>
    internal class NoCtrlCCancelLifetime : IHostLifetime, IDisposable
    {
        private PosixSignalRegistration? sigIntRegistration;
        private PosixSignalRegistration? sigQuitRegistration;
        private PosixSignalRegistration? sigTermRegistration;

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            RegisterShutdownHandlers();
            return Task.CompletedTask;

            void RegisterShutdownHandlers()
            {
                Action<PosixSignalContext> handler = static context => context.Cancel = true;
                sigIntRegistration = PosixSignalRegistration.Create(PosixSignal.SIGINT, handler);
                sigQuitRegistration = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, handler);
                sigTermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, handler);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void Dispose()
        {
            sigIntRegistration?.Dispose();
            sigQuitRegistration?.Dispose();
            sigTermRegistration?.Dispose();
        }
    }
}
