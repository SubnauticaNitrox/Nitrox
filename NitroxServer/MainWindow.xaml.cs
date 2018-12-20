using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;

namespace NitroxServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server _server;
        public static MainWindow Instance;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.SetOut(new ConsoleWriter());

            Instance = this;

            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            try
            {
                ServerConfig config = new ServerConfig();
                _server = new Server(config);
                _server.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Task.Factory.StartNew(()=> _server.Stop()); //Something locks the UI and everything freezes, thus a task for now.
        }

        public void WriteLog(string data)
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
        public void WriteLog(char data)
        {
            if (LogTextBox.Dispatcher.CheckAccess())
            {

                LogTextBox.Text += data;
                LogTextBox.SelectionStart = LogTextBox.Text.Length;
                LogTextBox.ScrollToEnd();

            }
            else
            {
                LogTextBox.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { WriteLog(data); }));
            }

        }
    }
}
