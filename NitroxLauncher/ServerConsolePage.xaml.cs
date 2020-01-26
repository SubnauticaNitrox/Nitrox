using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private readonly LauncherLogic logic;
        private string commandText = "";

        public string CommandText
        {
            get => commandText;
            set
            {
                commandText = value;
                OnPropertyChanged();
            }
        }

        private List<string> commandLinesHistory;
        private int commandLinesHistoryIndex;

        public ServerConsolePage(LauncherLogic logic)
        {
            InitializeComponent();
            PropertyChanged += OnPropertyChange;
            
            this.logic = logic;
            this.logic.ServerStarted += ServerStarted;
            this.logic.ServerDataReceived += ServerDataReceived;

            commandLinesHistory = new List<string>();
            commandLinesHistoryIndex = -1;

            OnPropertyChanged(nameof(CommandText));
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
            
            CommandText += inputText + Environment.NewLine;
            await logic.WriteToServerAsync(inputText);
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            CommandText = string.Empty;
        }

        private void ServerDataReceived(object sender, DataReceivedEventArgs e)
        {
            CommandText += e.Data + Environment.NewLine;
        }

        private async void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SendServerCommandWrapper();
        }

        private async void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Suggest referencing NitroxServer.ConsoleCommands.ExitCommand.name, but the class is internal
            await SendServerCommandAsync("exit");
            commandLinesHistory.Add("exit");
            commandLinesHistoryIndex = commandLinesHistory.Count;
        }

        private async Task SendServerCommandWrapper()
        {
            await SendServerCommandAsync(CommandLine.Text);
            commandLinesHistory.Add(CommandLine.Text);
            // Index is out of bounds after an entry
            commandLinesHistoryIndex = commandLinesHistory.Count;
            CommandLine.Text = string.Empty;
        }

        private async void CommandLine_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendServerCommandWrapper();
            }
            else if (e.Key == Key.Up)
            {
                if (commandLinesHistoryIndex > 0 && commandLinesHistoryIndex <= commandLinesHistory.Count)
                {
                    commandLinesHistoryIndex--;
                    CommandLine.Text = commandLinesHistory[commandLinesHistoryIndex];
                    CommandLine.SelectionStart = CommandLine.Text.Length;
                    CommandLine.SelectionLength = 0;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (commandLinesHistoryIndex >= 0 && commandLinesHistoryIndex < commandLinesHistory.Count - 1)
                {
                    commandLinesHistoryIndex++;
                    CommandLine.Text = commandLinesHistory[commandLinesHistoryIndex];
                    CommandLine.SelectionStart = CommandLine.Text.Length;
                    CommandLine.SelectionLength = 0;
                }
                else if (commandLinesHistoryIndex >= 0 && commandLinesHistoryIndex == commandLinesHistory.Count - 1)
                {
                    commandLinesHistoryIndex++;
                    CommandLine.Text = string.Empty;
                }
            }
        }

        private void OnPropertyChange(object sender, PropertyChangedEventArgs propertyName)
        {
            ServerConsolePage serverConsolePage = (ServerConsolePage)sender;
            Dispatcher?.BeginInvoke(new Action(() => serverConsolePage.ConsoleWindowScrollView.ScrollToEnd()));
        }
    }
}
