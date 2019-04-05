using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NitroxLauncher
{    
    public partial class ServerConsolePage : Page, INotifyPropertyChanged
    {
        private LauncherLogic logic;
        private Process serverProcess = null;
        private string commandText = "";

        public string CommandText
        {
            get
            {
                return commandText;
            }
            set
            {
                commandText = value;
                OnPropertyChanged();
            }
        }

        public ServerConsolePage(LauncherLogic logic)
        {
            InitializeComponent();
            this.logic = logic;
            this.logic.StartServerEvent += OnStartServer;
            CommandText = "";
        }        

        internal bool ServerRunning
        {
            get
            {
                return (serverProcess != null && !serverProcess.HasExited);
            }
        }

        private void OnStartServer(object sender, EventArgs e)
        {
            CommandText = "";
            serverProcess = (Process)sender;
            serverProcess.OutputDataReceived += HandleOutputData;
            serverProcess.BeginOutputReadLine();
            // Use another thread to signal when the server is shut down
            Thread thread = new Thread(new ThreadStart(() =>
            {
                serverProcess.WaitForExit();
                Thread.Sleep(1000);
                logic.EndServer();
            }));
            thread.Start();
            
        }

        private void HandleOutputData(object sender, DataReceivedEventArgs e)
        {
            CommandText += e.Data + "\n";
        }

        internal void HandleInputData(string inputText)
        {
            CommandText += inputText + "\n";
            serverProcess.StandardInput.WriteLineAsync(inputText);                        
        }

        private void CommandButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ServerRunning)
            {
                HandleInputData(CommandLine.Text);
                CommandLine.Text = "";
            }
        }

        private void CommandLine_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ServerRunning)
            {
                if (e.Key == Key.Enter)
                {
                    HandleInputData(CommandLine.Text);
                    CommandLine.Text = "";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}
