using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NitroxLauncher.Events;

namespace NitroxLauncher.Pages
{
    public partial class ServerConsolePage : PageBase, INotifyPropertyChanged
    {
        private readonly List<string> commandLinesHistory = new List<string>();
        private int commandHistoryIndex;
        private string commandInputText;
        private string serverOutput = string.Empty;

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

            LauncherLogic.Instance.ServerStarted += ServerStarted;
            LauncherLogic.Instance.ServerDataReceived += ServerDataReceived;

            Loaded += (sender, args) =>
            {
                CommandInput.Focus();
            };
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

        private async Task SendCommandInputToServerAsync()
        {
            await LauncherLogic.Instance.SendServerCommandAsync(CommandInputText);
            ServerOutput += CommandInputText + Environment.NewLine;

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

        private async void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SendCommandInputToServerAsync();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Suggest referencing NitroxServer.ConsoleCommands.ExitCommand.name, but the class is internal
            await LauncherLogic.Instance.SendServerCommandAsync("stop");
            ServerOutput += $"stop{Environment.NewLine}";

            commandLinesHistory.Add("stop");
            HideCommandHistory();
        }

        private async void CommandLine_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.Key)
            {
                case Key.Enter:
                    await SendCommandInputToServerAsync();
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

        private void PART_VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
