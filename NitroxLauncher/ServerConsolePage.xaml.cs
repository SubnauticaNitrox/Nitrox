using System;
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

        public ServerConsolePage(LauncherLogic logic)
        {
            InitializeComponent();
            PropertyChanged += OnPropertyChange;
            
            this.logic = logic;
            this.logic.ServerStarted += ServerStarted;
            this.logic.ServerDataReceived += ServerDataReceived;

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
            
            CommandText += inputText + "\n";
            await logic.WriteToServerAsync(inputText);
        }

        private void ServerStarted(object sender, ServerStartEventArgs e)
        {
            CommandText = "";
        }

        private void ServerDataReceived(object sender, DataReceivedEventArgs e)
        {
            CommandText += e.Data + "\n";
        }

        private async void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            await SendServerCommandAsync(CommandLine.Text);
            CommandLine.Text = "";
        }

        private async void CommandLine_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendServerCommandAsync(CommandLine.Text);
                CommandLine.Text = "";
            }
        }

        private void OnPropertyChange(object sender, PropertyChangedEventArgs propertyName)
        {
            ServerConsolePage serverConsolePage = (ServerConsolePage)sender;
            Dispatcher?.BeginInvoke(new Action(() => serverConsolePage.ConsoleWindowScrollView.ScrollToEnd()));
        }
    }
}
