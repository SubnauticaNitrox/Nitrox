using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NitroxLauncher.Events;

namespace NitroxLauncher
{
    public partial class ServerConsolePage : Page, INotifyPropertyChanged
    {
        private readonly List<string> commandLinesHistory = new List<string>();
        private readonly LauncherLogic logic;
        private int commandHistoryIndex;
        private string commandInputText;
        private string serverOutput = "";

        public string ServerOutput
        {
            get => serverOutput;
            set
            {
                serverOutput = value;
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
                    CommandInput.SelectionStart = CommandInputText.Length;
                    CommandInput.SelectionLength = 0;
                }
                OnPropertyChanged();
            }
        }

        public ServerConsolePage(LauncherLogic logic)
        {
            InitializeComponent();

            this.logic = logic;
            this.logic.ServerStarted += ServerStarted;
            this.logic.ServerDataReceived += ServerDataReceived;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal async Task SendServerCommandAsync(string inputText)
        {
            if (!logic.ServerRunning)
            {
                return;
            }

            ServerOutput += inputText + Environment.NewLine;
            await logic.WriteToServerAsync(inputText);
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            ServerOutput = string.Empty;
        }

        private void ServerDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: Change to virtualized textboxes per line.
            // This sucks for performance reasons. Every string concat in .NET will create a NEW string in memory.
            ServerOutput += e.Data + Environment.NewLine;
        }

        private async void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SendServerCommandAsync(CommandInputText);
            // Deduplication of command history
            if (CommandInputText != commandLinesHistory.LastOrDefault())
            {
                commandLinesHistory.Add(CommandInputText);
            }
            HideCommandHistory();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Suggest referencing NitroxServer.ConsoleCommands.ExitCommand.name, but the class is internal
            await SendServerCommandAsync("stop");
            commandLinesHistory.Add("stop");
            HideCommandHistory();
        }

        private void HideCommandHistory()
        {
            CommandHistoryIndex = commandLinesHistory.Count;
        }

        private void CommandLine_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    CommandButton_OnClick(sender, e);
                    break;
                case Key.Up:
                    CommandHistoryIndex++;
                    break;
                case Key.Down:
                    CommandHistoryIndex--;
                    break;
            }
        }
    }
}
