using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NitroxServer.Wpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConsoleExecutor Executor { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WriteLog(string data)
        {
            if (LogTextBox.Dispatcher.CheckAccess())
            {
                LogTextBox.Text += data + Environment.NewLine;
                LogTextBox.SelectionStart = LogTextBox.Text.Length;
                LogTextBox.ScrollToEnd();
            }
            else
            {
                LogTextBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { WriteLog(data); }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Process.GetProcessesByName(nameof(NitroxServer)).Any())
            {
                MessageBox.Show("Nitrox server has already been started. Please close it before starting a new server.");
                Environment.Exit(1);
            }

            Executor = ConsoleExecutor.Execute(typeof(Server).Assembly.Location);
            Executor.OutputReceived += (o, args) => WriteLog(args.Text);
            Executor.ErrorReceived += (o, args) => WriteLog(args.Text);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Hide();
            Executor.CloseProcess();
        }

        private void ServerInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ServerInputTextBox.Text))
            {
                return;
            }

            Executor.WriteLine(ServerInputTextBox.Text.Trim());
            ServerInputTextBox.Clear();
        }
    }
}
