using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using NitroxLauncher.Models;
using NitroxLauncher.Models.Events;

namespace NitroxLauncher.Pages
{
    public partial class ServerConsolePage : PageBase
    {
        private readonly List<string> commandLinesHistory = new();
        private readonly StringBuilder serverOutput = new("");
        private string commandInputText;
        private int commandHistoryIndex;

        public string ServerOutput
        {
            get => serverOutput.ToString();
            set
            {
                serverOutput.AppendLine(value);
                OnPropertyChanged();
                Dispatcher?.BeginInvoke(new Action(() => ConsoleWindowScrollView.ScrollToEnd()));
            }
        }

        public string CommandInputText
        {
            get => commandInputText;
            set
            {
                commandInputText = value;
                OnPropertyChanged();
            }
        }

        public int CommandHistoryIndex
        {
            get => commandHistoryIndex;
            set
            {
                commandHistoryIndex = Math.Min(Math.Max(value, 0), commandLinesHistory.Count);
                if (commandHistoryIndex >= commandLinesHistory.Count)
                {
                    // Out of bounds index means command history is disabled
                    CommandInputText = string.Empty;
                }
                else
                {
                    CommandInputText = commandLinesHistory[commandHistoryIndex];
                    // Move cursor at the end of the text
                    CommandInput.SelectionStart = CommandInputText.Length;
                    CommandInput.SelectionLength = 0;
                }

                OnPropertyChanged();
            }
        }

        public ServerConsolePage()
        {
            InitializeComponent();

            LauncherLogic.Server.ServerStarted += ServerStarted;
            LauncherLogic.Server.ServerDataReceived += ServerDataReceived;

            Loaded += (sender, args) =>
            {
                CommandInput.Focus();
            };
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            ServerOutput = string.Empty;
        }

        private void ServerDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: Change to virtualized textboxes per line.
            // This sucks for performance reasons. Every string concat in .NET will create a NEW string in memory.
            ServerOutput = e.Data;
        }

        private void SendCommandInputToServer()
        {
            ServerOutput = CommandInputText;
            LauncherLogic.Server.SendServerCommand(CommandInputText);

            // Deduplication of command history
            if (!string.IsNullOrWhiteSpace(CommandInputText) && CommandInputText != commandLinesHistory.LastOrDefault())
            {
                commandLinesHistory.Add(CommandInputText);
            }

            HideCommandHistory();
        }

        private void HideCommandHistory()
        {
            CommandHistoryIndex = commandLinesHistory.Count;
        }

        private void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            SendCommandInputToServer();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Suggest referencing NitroxServer.ConsoleCommands.ExitCommand.name, but the class is internal
            LauncherLogic.Server.SendServerCommand("stop");
            commandLinesHistory.Add("stop");
            HideCommandHistory();
        }

        private void CommandLine_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.Key)
            {
                case Key.Enter:
                    SendCommandInputToServer();
                    break;

                case Key.Escape:
                    HideCommandHistory();
                    break;

                case Key.Up:
                    CommandHistoryIndex--;
                    break;

                case Key.Down:
                    CommandHistoryIndex++;
                    break;

                default:
                    e.Handled = false;
                    break;
            }
        }
    }
}
