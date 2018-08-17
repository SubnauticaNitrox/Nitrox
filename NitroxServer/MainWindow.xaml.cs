using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace NitroxServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
                Server _Server = new Server();
                _Server.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
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

        public void NetworkSpeed(long download, long upload)
        {
            if (Dispatcher.CheckAccess())
            {
                DSpeed.Content = GetFileSizeKBPS(download);
                USpeed.Content = GetFileSizeKBPS(upload);
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { NetworkSpeed(download, upload); }));
            }
        }

        public static string GetFileSizeKBPS(long numBytes)
        {
            string fileSize = "";
            const int number = 1024;
            const double BytesInKilobytes = number;
            const double BytesInMegabytes = BytesInKilobytes * number;
            const double BytesInGigabytes = BytesInMegabytes * number;
            const double BytesInTerabytes = BytesInGigabytes * number;


            if (numBytes < BytesInTerabytes)
            {
                if (numBytes < BytesInGigabytes)
                {
                    if (numBytes < BytesInMegabytes)
                    {
                        if (numBytes < BytesInKilobytes)
                        {
                            fileSize = string.Format("{0:0.00} B/s", numBytes);
                        }
                        else
                        {
                            fileSize = string.Format("{0:0.00} KB/s", (numBytes / BytesInKilobytes));
                        }
                    }
                    else
                    {
                        fileSize = string.Format("{0:0.00} MB/s", numBytes / BytesInMegabytes);
                    }
                }
                else
                {
                    fileSize = string.Format("{0:0.00} GB/s", numBytes / BytesInGigabytes);
                }
            }
            else
            {
                fileSize = string.Format("{0:0.00} TB/s", numBytes / BytesInGigabytes);
            }

            return fileSize;
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
