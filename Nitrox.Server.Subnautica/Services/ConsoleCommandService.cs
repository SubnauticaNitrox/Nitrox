using System;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Reads console input and handles input history.
/// </summary>
internal sealed class ConsoleCommandService(CommandService commandService, PlayerRepository playerRepository, IServerPacketSender packetSender, ILogger<ConsoleCommandService> logger) : BackgroundService
{
    private readonly CommandService commandService = commandService;
    private readonly CircularBuffer<string> inputHistory = new(1000);
    private readonly ILogger<ConsoleCommandService> logger = logger;
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly IServerPacketSender packetSender = packetSender;
    private int currentHistoryIndex;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await HandleConsoleInputAsync(stoppingToken);

    private void SubmitInput(string input) => commandService.ExecuteCommand(input, new HostToServerCommandContext(packetSender));

    /// <summary>
    ///     Handles per-key input of the console.
    /// </summary>
    private async Task HandleConsoleInputAsync(CancellationToken ct = default)
    {
        if (Console.IsInputRedirected)
        {
            logger.ZLogDebug($"Input stream is redirected");
            await Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    SubmitInput(Console.ReadLine());
                }
            }, ct).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.ZLogError(t.Exception, $"Failed to read console input");
                }
            }, ct);
        }
        else
        {
            logger.ZLogDebug($"Input stream is available");
            StringBuilder inputLineBuilder = new();

            void ClearInputLine()
            {
                currentHistoryIndex = 0;
                inputLineBuilder.Clear();
                Console.Write($"\r{new string(' ', Console.WindowWidth - 1)}\r");
            }

            void RedrawInput(int start = 0, int end = 0)
            {
                int lastPosition = Console.CursorLeft;
                // Expand range to end if end value is -1
                if (start > -1 && end == -1)
                {
                    end = Math.Max(inputLineBuilder.Length - start, 0);
                }

                if (start == 0 && end == 0)
                {
                    // Redraw entire line
                    Console.Write($"\r{new string(' ', Console.WindowWidth - 1)}\r{inputLineBuilder}");
                }
                else
                {
                    // Redraw part of line
                    string changedInputSegment = inputLineBuilder.ToString(start, end);
                    Console.CursorVisible = false;
                    Console.Write($"{changedInputSegment}{new string(' ', inputLineBuilder.Length - changedInputSegment.Length - Console.CursorLeft + 1)}");
                    Console.CursorVisible = true;
                }
                Console.CursorLeft = lastPosition;
            }

            await Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    if (!Console.KeyAvailable)
                    {
                        try
                        {
                            await Task.Delay(10, ct);
                        }
                        catch (TaskCanceledException)
                        {
                            // ignored
                        }
                        continue;
                    }

                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

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
                                if (Console.CursorLeft + 4 < Console.WindowWidth)
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
                                Console.CursorLeft = Math.Min(inputLineBuilder.Length, Console.WindowWidth);
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
                                Console.CursorLeft = Math.Min(inputLineBuilder.Length, Console.WindowWidth);
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
            }, ct).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.LogError(t.Exception, "Failed to handle console input");
                }
            }, ct);
        }
    }
}
